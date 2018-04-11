using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public sealed class ChainIndexer : AbstractAzureIndexer
    {
        public override CheckpointType CheckPointType { get; } = CheckpointType.Chain;

        private readonly IChainClient _chainClient;

        private readonly ILogger _logger;

        public ChainIndexer(
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings,
            IChainClient chainClient,
            ILoggerFactory loggerFactory)
            : base(fullNode, chain, storageClient, settings)
        {
            TaskScheduler = new CustomThreadPoolTaskScheduler(30, 100);
            _chainClient = chainClient;
            _logger = loggerFactory.CreateLogger<ChainIndexer>();
        }

        /// <summary>
        /// Performs "Chain" indexing into Azure storage.
        /// </summary>
        /// <param name="cancellationToken">The token used for cancellation.</param>
        /// <returns>A task for asynchronous completion.</returns>
        public override async Task IndexAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("()");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Index(Chain, cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                        .ContinueWith(t => { }, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                        .ContinueWith(t => { }, cancellationToken).ConfigureAwait(false);
                }
            }

            _logger.LogTrace("(-)");
        }

        public void Index(ChainBase chain, CancellationToken cancellationToken)
        {
            if (chain == null)
            {
                throw new ArgumentNullException("chain");
            }

            SetThrottling();

            using (IndexerTrace.NewCorrelation("IndexTransactions main chain to azure started"))
            {
                IndexerTrace.InputChainTip(chain.Tip);

                var changes = _chainClient.GetChainChangesUntilFork(chain.Tip, true, cancellationToken).ToList();
                
                var height = 0;
                if (changes.Count != 0)
                {
                    IndexerTrace.IndexedChainTip(changes[0].BlockId, changes[0].Height);
                    if (changes[0].Height > chain.Tip.Height)
                    {
                        IndexerTrace.InputChainIsLate();
                        return;
                    }

                    height = changes[changes.Count - 1].Height + 1;
                    if (height > chain.Height)
                    {
                        IndexerTrace.IndexedChainIsUpToDate(chain.Tip);
                        return;
                    }
                }
                else
                {
                    IndexerTrace.NoForkFoundWithStored();
                }

                IndexerTrace.IndexingChain(chain.GetBlock(height), chain.Tip);
                Index(chain, height, cancellationToken);
            }
        }

        private void Index(ChainBase chain, int startHeight, CancellationToken cancellationToken)
        {
            var capacity = (chain.Height - startHeight) + 1;
            var entries = new List<ChainPartEntity>(capacity);

            for (var i = startHeight; i <= chain.Tip.Height; i++)
            {
                var block = chain.GetBlock(i);
                var chainPart = new ChainPartEntity
                {
                    Height = i,
                    BlockHeader = block.Header
                };
                entries.Add(chainPart);
            }

            Index(entries, cancellationToken);
        }

        private void Index(IReadOnlyList<ChainPartEntity> chainParts, CancellationToken cancellationToken)
        {
            if (!chainParts.Any())
            {
                return;
            }

            var table = StorageClient.GetChainTable();
            var batch = new TableBatchOperation();
            var first = chainParts.First();
            var last = chainParts.Last();

            Tip = Chain.GetBlock(first.Height);

            foreach (var entry in chainParts)
            {
                batch.Add(TableOperation.InsertOrReplace(entry.ToEntity()));
                if (batch.Count == Settings.BatchSize)
                {
                    table.ExecuteBatchAsync(batch).GetAwaiter().GetResult();
                    batch = new TableBatchOperation();
                    Tip = Chain.GetBlock(entry.Height);
                }
                IndexerTrace.RemainingBlockChain(entry.Height, last.Height);
            }

            if (batch.Count <= 0)
            {
                return;
            }

            table.ExecuteBatchAsync(batch, null, null, cancellationToken).GetAwaiter().GetResult();
            Tip = Chain.GetBlock(last.Height);
        }
    }
}
