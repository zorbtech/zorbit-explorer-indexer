using Microsoft.WindowsAzure.Storage.Table;

namespace Stratis.Bitcoin.Features.AzureIndexer.IndexTasks
{
    public class IndexTransactionsTask : IndexTableEntitiesTaskBase<TransactionEntry.Entity>
    {
        public IndexTransactionsTask(IndexerConfiguration configuration)
            : base(configuration)
        {
        }


        protected override void ProcessBlock(BlockInfo block, BulkImport<TransactionEntry.Entity> bulk)
        {
            foreach (var transaction in block.Block.Transactions)
            {
                var indexed = new TransactionEntry.Entity(null, transaction, block.BlockId);
                bulk.Add(indexed.PartitionKey, indexed);
            }
        }

        protected override CloudTable GetCloudTable()
        {
            return Configuration.GetTransactionTable();
        }

        protected override ITableEntity ToTableEntity(TransactionEntry.Entity indexed)
        {
            return indexed.CreateTableEntity();
        }
    }
}
