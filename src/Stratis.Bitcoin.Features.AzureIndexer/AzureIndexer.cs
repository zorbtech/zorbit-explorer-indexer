using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Balance;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;
using Stratis.Bitcoin.Features.AzureIndexer.Wallet;
using Stratis.Bitcoin.Features.Consensus.Rules;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    public enum IndexerCheckpoints
    {
        Wallets,
        Transactions,
        Blocks,
        Balances
    }

    public class AzureIndexer
    {
        public AzureIndexer(IndexerConfiguration configuration)
        {
            this.TaskScheduler = TaskScheduler.Default;
            this.CheckpointInterval = TimeSpan.FromMinutes(15.0);
            this.Configuration = configuration ?? throw new ArgumentNullException("configuration");
            this.FromHeight = 0;
            this.ToHeight = int.MaxValue;
        }

        public void Index(params Block[] blocks)
        {
            var task = new IndexBlocksTask(this.Configuration);
            task.Index(blocks, this.TaskScheduler);
        }

        public Task IndexAsync(params Block[] blocks)
        {
            var task = new IndexBlocksTask(this.Configuration);
            return task.IndexAsync(blocks, this.TaskScheduler);
        }

        public void Index(params TransactionEntry.Entity[] entities)
        {
            this.Index(entities.Select(e => e.CreateTableEntity()).ToArray(), this.Configuration.GetTransactionTable());
        }

        public Task IndexAsync(params TransactionEntry.Entity[] entities)
        {
            return this.IndexAsync(entities.Select(e => e.CreateTableEntity()).ToArray(), this.Configuration.GetTransactionTable());
        }

        public void Index(IEnumerable<OrderedBalanceChange> balances)
        {
            this.Index(balances.Select(b => b.ToEntity()), this.Configuration.GetBalanceTable());
        }

        public Task IndexAsync(IEnumerable<OrderedBalanceChange> balances)
        {
            return this.IndexAsync(balances.Select(b => b.ToEntity()), this.Configuration.GetBalanceTable());
        }

        public void IndexChain(ChainBase chain, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (chain == null)
            {
                throw new ArgumentNullException("chain");
            }

            this.SetThrottling();

            using (IndexerTrace.NewCorrelation("Index main chain to azure started"))
            {
                IndexerTrace.InputChainTip(chain.Tip);

                var client = this.Configuration.CreateIndexerClient();
                var changes = client.GetChainChangesUntilFork(chain.Tip, true, cancellationToken).ToList();

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
                this.Index(chain, height, cancellationToken);
            }
        }

        public Checkpoint GetCheckpoint(IndexerCheckpoints checkpoint)
        {
            return this.GetCheckpointRepository().GetCheckpoint(checkpoint.ToString().ToLowerInvariant());
        }

        public Task<Checkpoint> GetCheckpointAsync(IndexerCheckpoints checkpoint)
        {
            return this.GetCheckpointRepository().GetCheckpointAsync(checkpoint.ToString().ToLowerInvariant());
        }

        public CheckpointRepository GetCheckpointRepository()
        {
            return new CheckpointRepository(this.Configuration.GetBlocksContainer(),
                this.Configuration.Network, string.IsNullOrWhiteSpace(this.Configuration.CheckpointSetName)
                ? "default" : this.Configuration.CheckpointSetName);
        }

        public void IndexOrderedBalance(int height, Block block)
        {
            if (block == null)
            {
                return;
            }

            var table = this.Configuration.GetBalanceTable();
            var blockId = block?.GetHash();
            var header = block?.Header;

            var entities =
                block
                    .Transactions
                    .SelectMany(t => OrderedBalanceChange.ExtractScriptBalances(t.GetHash(), t, blockId, header, height))
                    .Select(_ => _.ToEntity())
                    .AsEnumerable();

            Index(entities, table);
        }

        public void IndexTransactions(int height, Block block)
        {
            if (block == null)
            {
                return;
            }

            var table = this.Configuration.GetTransactionTable();
            var blockId = block.GetHash();

            var entities =
                        block
                        .Transactions
                        .Select(t => new TransactionEntry.Entity(t.GetHash(), t, blockId))
                        .Select(c => c.CreateTableEntity())
                        .AsEnumerable();

            this.Index(entities, table);
        }

        public void IndexWalletOrderedBalance(int height, Block block, WalletRuleEntryCollection walletRules)
        {
            try
            {
                this.IndexWalletOrderedBalanceAsync(height, block, walletRules).Wait();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
        }

        public Task IndexWalletOrderedBalanceAsync(int height, Block block, WalletRuleEntryCollection walletRules)
        {
            if (block == null)
            {
                return Task.CompletedTask;
            }

            var table = this.Configuration.GetBalanceTable();
            var blockId = block?.GetHash();

            var entities =
                    block
                    .Transactions
                    .SelectMany(t => OrderedBalanceChange.ExtractWalletBalances(null, t, blockId, block.Header, height, walletRules))
                    .Select(t => t.ToEntity())
                    .AsEnumerable();

            return this.IndexAsync(entities, table);
        }

        public void IndexOrderedBalance(Transaction tx)
        {
            var table = this.Configuration.GetBalanceTable();
            var entities = OrderedBalanceChange.ExtractScriptBalances(tx).Select(t => t.ToEntity()).AsEnumerable();
            this.Index(entities, table);
        }

        public Task IndexOrderedBalanceAsync(Transaction tx)
        {
            var table = this.Configuration.GetBalanceTable();
            var entities = OrderedBalanceChange.ExtractScriptBalances(tx).Select(t => t.ToEntity()).AsEnumerable();
            return this.IndexAsync(entities, table);
        }

        internal Checkpoint GetCheckpointInternal(IndexerCheckpoints checkpoint)
        {
            var chk = this.GetCheckpoint(checkpoint);
            if (this.IgnoreCheckpoints)
                chk = new Checkpoint(chk.CheckpointName, this.Configuration.Network, null, null);
            return chk;
        }

        internal ChainBase GetMainChain()
        {
            return this.Configuration.CreateIndexerClient().GetMainChain();
        }

        internal void Index(ChainBase chain, int startHeight, CancellationToken cancellationToken = default(CancellationToken))
        {
            var capacity = ((chain.Height - startHeight) / BlockHeaderPerRow) + (BlockHeaderPerRow == 1 ? 1 : BlockHeaderPerRow - 1);
            var entries = new List<ChainPartEntry>(capacity);
            startHeight = startHeight - (startHeight % BlockHeaderPerRow);
            ChainPartEntry chainPart = null;

            for (var i = startHeight; i <= chain.Tip.Height; i++)
            {
                if (chainPart == null)
                {
                    chainPart = new ChainPartEntry()
                    {
                        ChainOffset = i
                    };
                }

                var block = chain.GetBlock(i);
                chainPart.BlockHeaders.Add(block.Header);

                if (chainPart.BlockHeaders.Count != BlockHeaderPerRow)
                {
                    continue;
                }

                entries.Add(chainPart);
                chainPart = null;
            }

            if (chainPart != null)
            {
                entries.Add(chainPart);
            }

            this.Index(entries, cancellationToken);
        }

        private void Index(IEnumerable<ITableEntity> entities, CloudTable table)
        {
            var task = new IndexTableEntitiesTask(this.Configuration, table);
            task.Index(entities, this.TaskScheduler);
        }

        private Task IndexAsync(IEnumerable<ITableEntity> entities, CloudTable table)
        {
            var task = new IndexTableEntitiesTask(this.Configuration, table);
            return task.IndexAsync(entities, this.TaskScheduler);
        }

        private void SetThrottling()
        {
            Helper.SetThrottling();
            var tableServicePoint = ServicePointManager.FindServicePoint(this.Configuration.TableClient.BaseUri);
            tableServicePoint.ConnectionLimit = 1000;
        }

        private void PushTransactions(MultiValueDictionary<string, TransactionEntry.Entity> buckets,
            IEnumerable<TransactionEntry.Entity> indexedTransactions,
            BlockingCollection<TransactionEntry.Entity[]> transactions)
        {
            var array = indexedTransactions.ToArray();
            transactions.Add(array);
            buckets.Remove(array[0].PartitionKey);
        }

        private void Index(IReadOnlyList<ChainPartEntry> chainParts, CancellationToken cancellationToken = default(CancellationToken))
        {
            var table = this.Configuration.GetChainTable();
            var batch = new TableBatchOperation();
            var last = chainParts[chainParts.Count - 1];

            foreach (var entry in chainParts)
            {
                batch.Add(TableOperation.InsertOrReplace(entry.ToEntity()));
                if (batch.Count == ChainBatchUploadCount)
                {
                    table.ExecuteBatchAsync(batch).GetAwaiter().GetResult();
                    batch = new TableBatchOperation();
                }
                IndexerTrace.RemainingBlockChain(entry.ChainOffset, last.ChainOffset + last.BlockHeaders.Count - 1);
            }

            if (batch.Count > 0)
            {
                table.ExecuteBatchAsync(batch, null, null, cancellationToken).GetAwaiter().GetResult();
            }
        }

        public IndexerConfiguration Configuration { get; }

        public TaskScheduler TaskScheduler { get; set; }

        public TimeSpan CheckpointInterval { get; set; }

        public int FromHeight { get; set; }

        public bool IgnoreCheckpoints { get; set; }

        public int ToHeight { get; set; }

        internal int BlockHeaderPerRow { get; set; } = 6;

        internal int ChainBatchUploadCount { get; set; } = 100;
    }
}
