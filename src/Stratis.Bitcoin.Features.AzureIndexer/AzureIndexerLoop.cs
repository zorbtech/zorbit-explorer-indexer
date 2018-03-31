using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    /// <summary>
    /// The AzureIndexerStoreLoop loads blocks from the block repository and indexes them in Azure.
    /// </summary>
    public class AzureIndexerLoop
    {
        /// <summary>Factory for creating background async loop tasks.</summary>
        private readonly IAsyncLoopFactory _asyncLoopFactory;

        /// <summary>The async loop we need to wait upon before we can shut down this feature.</summary>
        private IAsyncLoop _asyncLoop;

        /// <summary>Another async loop we need to wait upon before we can shut down this feature.</summary>
        private IAsyncLoop _asyncLoopChain;

        /// <summary>The full node that owns the block repository that we want to index.</summary>
        public FullNode FullNode { get; }

        /// <summary>Best chain of block headers.</summary>
        internal readonly ConcurrentChain Chain;

        /// <summary>Instance logger.</summary>
        private readonly ILogger _logger;

        /// <summary>The node life time let us know when the node is shutting down.</summary>
        private readonly INodeLifetime _nodeLifetime;

        /// <summary>The number of blocks to index at a time.</summary>
        private const int IndexBatchSize = 100;

        /// <summary>The name of this node feature for reporting stats.</summary>
        public virtual string StoreName { get { return "AzureIndexer"; } }

        /// <summary>The Azure Indezer settings.</summary>
        private readonly AzureIndexerSettings _indexerSettings;

        /// <summary>The highest block that has been indexed.</summary>
        internal ChainedBlock StoreTip { get; private set; }

        /// <summary>The Azure Indexer.</summary>
        public AzureIndexer AzureIndexer { get; private set; }

        public BlockFetcher BlocksFetcher { get; private set; }

        public BlockFetcher TransactionsFetcher { get; private set; }

        public BlockFetcher BalancesFetcher { get; private set; }

        public BlockFetcher WalletsFetcher { get; private set; }

        /// <summary>The Indexer Configuration.</summary>
        public IndexerConfiguration IndexerConfig { get; private set; }

        /// <summary>
        /// Constructs the AzureIndexerLoop.
        /// </summary>
        /// <param name="fullNode">The full node that will be indexed.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public AzureIndexerLoop(FullNode fullNode, ILoggerFactory loggerFactory)
        {
            this._asyncLoopFactory = fullNode.AsyncLoopFactory;
            this.FullNode = fullNode;
            this.Chain = fullNode.Chain;
            this._nodeLifetime = fullNode.NodeLifetime;
            this._indexerSettings = fullNode.NodeService<AzureIndexerSettings>();
            this._logger = loggerFactory.CreateLogger(GetType().FullName);
        }

        /// <summary>
        /// Derives an IndexerConfiguration object from the proviced AzureIndexerSettings object and network.
        /// </summary>
        /// <param name="indexerSettings">The AzureIndexerSettings object to use.</param>
        /// <param name="network">The network to use.</param>
        /// <returns>An IndexerConfiguration object derived from the AzureIndexerSettings object and network.</returns>
        public static IndexerConfiguration IndexerConfigFromSettings(AzureIndexerSettings indexerSettings, Network network)
        {
            var indexerConfig = new IndexerConfiguration
            {
                StorageNamespace = indexerSettings.StorageNamespace,
                Network = network,
                CheckpointSetName = indexerSettings.CheckpointsetName,
                AzureStorageEmulatorUsed = indexerSettings.AzureEmulatorUsed,
                AzureConnectionString = indexerSettings.AzureEmulatorUsed ? null : indexerSettings.AzureConnectionString
            };
            return indexerConfig;
        }

        /// <summary>
        /// Initializes the Azure Indexer.
        /// </summary>
        public void Initialize()
        {
            this._logger.LogTrace("()");

            this.IndexerConfig = IndexerConfigFromSettings(this._indexerSettings, this.FullNode.Network);

            var indexer = this.IndexerConfig.CreateIndexer();

            SetupAzureStorage(indexer);

            indexer.TaskScheduler = new CustomThreadPoolTaskScheduler(30, 100);
            indexer.CheckpointInterval = this._indexerSettings.CheckpointInterval;
            indexer.IgnoreCheckpoints = this._indexerSettings.IgnoreCheckpoints;
            indexer.FromHeight = this._indexerSettings.From;
            indexer.ToHeight = this._indexerSettings.To;
            this.AzureIndexer = indexer;

            var checkpointBlocks = this.GetCheckPointBlock(IndexerCheckpoints.Blocks);
            var checkpointBalances = this.GetCheckPointBlock(IndexerCheckpoints.Balances);
            var checkpointTransactions = this.GetCheckPointBlock(IndexerCheckpoints.Transactions);
            var checkpointWallets = this.GetCheckPointBlock(IndexerCheckpoints.Wallets);

            if (this._indexerSettings.IgnoreCheckpoints)
            {
                this.SetStoreTip(this.Chain.GetBlock(indexer.FromHeight));
            }
            else
            {
                var minHeight = checkpointBlocks.Height;
                minHeight = Math.Min(minHeight, checkpointBalances.Height);
                minHeight = Math.Min(minHeight, checkpointTransactions.Height);
                minHeight = Math.Min(minHeight, checkpointWallets.Height);

                this.SetStoreTip(this.Chain.GetBlock(minHeight));
            }

            this.BlocksFetcher = this.GetBlockFetcher(IndexerCheckpoints.Blocks, this._nodeLifetime.ApplicationStopping, checkpointBlocks);
            this.BalancesFetcher = this.GetBlockFetcher(IndexerCheckpoints.Balances, this._nodeLifetime.ApplicationStopping, checkpointBalances);
            this.TransactionsFetcher = this.GetBlockFetcher(IndexerCheckpoints.Transactions, this._nodeLifetime.ApplicationStopping, checkpointTransactions);
            this.WalletsFetcher = this.GetBlockFetcher(IndexerCheckpoints.Wallets, this._nodeLifetime.ApplicationStopping, checkpointWallets);

            this.StartLoop();

            this._logger.LogTrace("(-)");
        }

        /// <summary>
        /// Shuts down the indexing loop.
        /// </summary>
        public void Shutdown()
        {
            this._asyncLoop.Dispose();
            this._asyncLoopChain.Dispose();
        }

        private void SetupAzureStorage(AzureIndexer indexer)
        {
            if (this._indexerSettings.ResetStorage)
            {
                this._logger.LogInformation("Clearing Azure Storage");
                indexer.Configuration.Teardown();
                this._logger.LogInformation("Azure Storage Cleared");
            }

            if (this._indexerSettings.ResetStorage)
            {
                this._logger.LogInformation("Validating Azure Storage");
            }

            indexer.Configuration.EnsureSetup();

            if (this._indexerSettings.ResetStorage)
            {
                this._logger.LogInformation("Azure Storage Validated");
            }
        }

        /// <summary>
        /// Determines the block that a checkpoint is at.
        /// </summary>
        /// <param name="indexerCheckpoints">The type of checkpoint (wallets, blocks, transactions or balances).</param>
        /// <returns>The block that a checkpoint is at.</returns>
        private ChainedBlock GetCheckPointBlock(IndexerCheckpoints indexerCheckpoints)
        {
            var checkpoint = this.AzureIndexer.GetCheckpointInternal(indexerCheckpoints);
            return this.Chain.FindFork(checkpoint.BlockLocator);
        }

        /// <summary>
        /// Starts the indexing loop.
        /// </summary>
        private void StartLoop()
        {
            this._asyncLoopChain = this._asyncLoopFactory.Run($"{this.StoreName}.IndexChainAsync", async token =>
                {
                    await IndexChainAsync(this._nodeLifetime.ApplicationStopping);
                },
                this._nodeLifetime.ApplicationStopping,
                TimeSpans.RunOnce,
                TimeSpans.Minute);

            this._asyncLoop = this._asyncLoopFactory.Run($"{this.StoreName}.IndexAsync", async token =>
                {
                    await IndexAsync(this._nodeLifetime.ApplicationStopping);
                },
                this._nodeLifetime.ApplicationStopping,
                TimeSpans.RunOnce,
                TimeSpans.FiveSeconds);
        }

        /// <summary>
        /// Gets a block fetcher that respects the given type of checkpoint.
        /// The block fetcher will return "IndexBatchSize" blocks starting at this.StoreTip + 1.
        /// If "this.AzureIndexer.IgnoreCheckpoints" is set then the checkpoints 
        /// will be ignored by "GetCheckpointInternal".
        /// </summary>
        /// <param name="indexerCheckpoints">The type of checkpoint (wallets, blocks, transactions or balances).</param>
        /// <param name="cancellationToken">The token used for cancellation.</param>
        /// <returns>A block fetcher that respects the given type of checkpoint.</returns>
        private BlockFetcher GetBlockFetcher(IndexerCheckpoints indexerCheckpoints, CancellationToken cancellationToken, ChainedBlock lastProcessed)
        {
            var checkpoint = this.AzureIndexer.GetCheckpointInternal(indexerCheckpoints);
            var repo = new FullNodeBlocksRepository(this.FullNode);
            return new BlockFetcher(checkpoint, repo, this.Chain, lastProcessed)
            {
                NeedSaveInterval = this._indexerSettings.CheckpointInterval,
                FromHeight = this.StoreTip.Height + 1,
                ToHeight = Math.Min(this.StoreTip.Height + IndexBatchSize, this._indexerSettings.To),
                CancellationToken = cancellationToken
            };
        }

        /// <summary>
        /// Performs "Chain" indexing into Azure storage.
        /// </summary>
        /// <param name="cancellationToken">The token used for cancellation.</param>
        /// <returns>A task for asynchronous completion.</returns>
        private async Task IndexChainAsync(CancellationToken cancellationToken)
        {
            this._logger.LogTrace("()");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    this.AzureIndexer.IndexChain(this.Chain, cancellationToken);
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken)
                        .ContinueWith(t => { }, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // If something goes wrong then try again 1 minute later
                    IndexerTrace.ErrorWhileImportingBlockToAzure(this.StoreTip.HashBlock, ex);
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken)
                        .ContinueWith(t => { }, cancellationToken).ConfigureAwait(false);
                }
            }

            this._logger.LogTrace("(-)");
        }

        /// <summary>
        /// Performs indexing into Azure storage.
        /// </summary>
        /// <param name="cancellationToken">The token used for cancellation.</param>
        /// <returns>A task for asynchronous completion.</returns>
        private async Task IndexAsync(CancellationToken cancellationToken)
        {
            this._logger.LogTrace("()");

            while (this.StoreTip.Height < _indexerSettings.To && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // All indexes will progress more or less in step
                    var fromHeight = this.StoreTip.Height + 1;
                    var toHeight = Math.Min(this.StoreTip.Height + IndexBatchSize, this._indexerSettings.To);

                    var tasks = new List<Task>
                    {
                        IndexBlocksAsync(fromHeight, toHeight, cancellationToken),
                        IndexTransactionsAsync(fromHeight, toHeight, cancellationToken),
                        IndexBalancesAsync(fromHeight, toHeight, cancellationToken),
                        IndexWalletsAsync(fromHeight, toHeight, cancellationToken),
                    };
                    Task.WaitAll(tasks.ToArray());

                    // Update the StoreTip value from the minHeight
                    var minHeight = this.BlocksFetcher.LastProcessed.Height;
                    minHeight = Math.Min(minHeight, this.BalancesFetcher.LastProcessed.Height);
                    minHeight = Math.Min(minHeight, this.TransactionsFetcher.LastProcessed.Height);
                    minHeight = Math.Min(minHeight, this.WalletsFetcher.LastProcessed.Height);

                    this.SetStoreTip(this.Chain.GetBlock(minHeight));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // If something goes wrong then try again 1 minute later
                    this._logger.LogError(ex.Message);
                    IndexerTrace.ErrorWhileImportingBlockToAzure(this.StoreTip.HashBlock, ex);
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                        .ContinueWith(t => { }, cancellationToken).ConfigureAwait(false);
                }
            }

            this._logger.LogTrace("(-)");
        }

        /// <summary>
        /// Sets the StoreTip.
        /// </summary>
        /// <param name="chainedBlock">The block to set the store tip to.</param>
        private void SetStoreTip(ChainedBlock chainedBlock)
        {
            this._logger.LogTrace("({0}:'{1}')", nameof(chainedBlock), chainedBlock?.HashBlock);
            Guard.NotNull(chainedBlock, nameof(chainedBlock));
            this.StoreTip = chainedBlock;
            this._logger.LogTrace("(-)");
        }

        private Task IndexBlocksAsync(int fromHeight, int toHeight, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || toHeight <= this.BlocksFetcher.LastProcessed.Height)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                this.BlocksFetcher.FromHeight = Math.Max(this.BlocksFetcher.LastProcessed.Height + 1, fromHeight);
                this.BlocksFetcher.ToHeight = toHeight;
                var task = new IndexBlocksTask(this.IndexerConfig)
                {
                    SaveProgression = !this._indexerSettings.IgnoreCheckpoints
                };
                task.Index(this.BlocksFetcher, this.AzureIndexer.TaskScheduler);
            }, cancellationToken);
        }

        private Task IndexTransactionsAsync(int fromHeight, int toHeight, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || toHeight <= this.TransactionsFetcher.LastProcessed.Height)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                this.TransactionsFetcher.FromHeight = Math.Max(this.TransactionsFetcher.LastProcessed.Height + 1, fromHeight);
                this.TransactionsFetcher.ToHeight = toHeight;
                var task = new IndexTransactionsTask(this.IndexerConfig)
                {
                    SaveProgression = !this._indexerSettings.IgnoreCheckpoints
                };
                task.Index(this.TransactionsFetcher, this.AzureIndexer.TaskScheduler);
            }, cancellationToken);
        }

        private Task IndexBalancesAsync(int fromHeight, int toHeight, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || toHeight <= this.BalancesFetcher.LastProcessed.Height)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                this.BalancesFetcher.FromHeight = Math.Max(this.BalancesFetcher.LastProcessed.Height + 1, fromHeight);
                this.BalancesFetcher.ToHeight = toHeight;
                var task = new IndexBalanceTask(this.IndexerConfig, null)
                {
                    SaveProgression = !this._indexerSettings.IgnoreCheckpoints
                };
                task.Index(this.BalancesFetcher, this.AzureIndexer.TaskScheduler);
            }, cancellationToken);
        }

        private Task IndexWalletsAsync(int fromHeight, int toHeight, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || toHeight <= this.WalletsFetcher.LastProcessed.Height)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                this.WalletsFetcher.FromHeight = Math.Max(this.WalletsFetcher.LastProcessed.Height + 1, fromHeight);
                this.WalletsFetcher.ToHeight = toHeight;
                var task = new IndexBalanceTask(this.IndexerConfig, this.IndexerConfig.CreateIndexerClient().GetAllWalletRules())
                {
                    SaveProgression = !this._indexerSettings.IgnoreCheckpoints
                };
                task.Index(this.WalletsFetcher, this.AzureIndexer.TaskScheduler);
            }, cancellationToken);
        }
    }
}