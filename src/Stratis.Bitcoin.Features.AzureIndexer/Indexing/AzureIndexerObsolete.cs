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

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    public class AzureIndexerObsolete : AbstractAzureIndexer
    {
        public override CheckpointType CheckPointType { get; } = CheckpointType.Chain;

        public AzureIndexerObsolete( 
            FullNode fullNode,
            ConcurrentChain chain,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings) 
            : base(fullNode, chain, storageClient, settings)
        {
        }

        public override async Task IndexAsync(CancellationToken cancellationToken)
        {
        }

        //public void IndexTransactions(params Block[] blocks)
        //{
        //    var task = new BlockBlobTask(this.StorageClient);
        //    task.IndexTransactions(blocks, this.TaskScheduler);
        //}

        //public Task IndexTransactionsAsync(params Block[] blocks)
        //{
        //    var task = new BlockBlobTask(this.StorageClient);
        //    return task.IndexTransactionsAsync(blocks, this.TaskScheduler);
        //}        

        public void IndexTransactions(params TransactionEntry.Entity[] entities)
        {
            this.IndexAsync(entities.Select(e => e.CreateTableEntity()).ToArray(), this.StorageClient.GetTransactionTable()).GetAwaiter().GetResult();
        }

        public Task IndexTransactionsAsync(params TransactionEntry.Entity[] entities)
        {
            return this.IndexAsync(entities.Select(e => e.CreateTableEntity()).ToArray(), this.StorageClient.GetTransactionTable());
        }

        public void IndexTransactions(IEnumerable<OrderedBalanceChange> balances)
        {
            this.IndexAsync(balances.Select(b => b.ToEntity()), this.StorageClient.GetBalanceTable()).GetAwaiter().GetResult();
        }

        public Task IndexTransactionsAsync(IEnumerable<OrderedBalanceChange> balances)
        {
            return this.IndexAsync(balances.Select(b => b.ToEntity()), this.StorageClient.GetBalanceTable());
        }

        public void IndexOrderedBalance(int height, Block block)
        {
            if (block == null)
            {
                return;
            }

            var table = this.StorageClient.GetBalanceTable();
            var blockId = block?.GetHash();
            var header = block?.Header;

            var entities =
                block
                    .Transactions
                    .SelectMany(t => OrderedBalanceChange.ExtractScriptBalances(t.GetHash(), t, blockId, header, height))
                    .Select(_ => _.ToEntity())
                    .AsEnumerable();

            IndexAsync(entities, table).GetAwaiter().GetResult();
        }

        public void IndexTransactions(int height, Block block)
        {
            if (block == null)
            {
                return;
            }

            var table = this.StorageClient.GetTransactionTable();
            var blockId = block.GetHash();

            var entities =
                        block
                        .Transactions
                        .Select(t => new TransactionEntry.Entity(t.GetHash(), t, blockId))
                        .Select(c => c.CreateTableEntity())
                        .AsEnumerable();

            IndexAsync(entities, table).GetAwaiter().GetResult();
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

            var table = this.StorageClient.GetBalanceTable();
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
            var table = this.StorageClient.GetBalanceTable();
            var entities = OrderedBalanceChange.ExtractScriptBalances(tx).Select(t => t.ToEntity()).AsEnumerable();
            IndexAsync(entities, table).GetAwaiter().GetResult();
        }

        public Task IndexOrderedBalanceAsync(Transaction tx)
        {
            var table = this.StorageClient.GetBalanceTable();
            var entities = OrderedBalanceChange.ExtractScriptBalances(tx).Select(t => t.ToEntity()).AsEnumerable();
            return this.IndexAsync(entities, table);
        }

        internal ChainBase GetMainChain()
        {
            return new IndexerClient(FullNode, Chain, StorageClient, Settings).GetMainChain();
        }

        private void PushTransactions(MultiValueDictionary<string, TransactionEntry.Entity> buckets,
            IEnumerable<TransactionEntry.Entity> indexedTransactions,
            BlockingCollection<TransactionEntry.Entity[]> transactions)
        {
            var array = indexedTransactions.ToArray();
            transactions.Add(array);
            buckets.Remove(array[0].PartitionKey);
        }
        
        internal int ChainBatchUploadCount { get; set; } = 100;
    }
}
