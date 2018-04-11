using Microsoft.WindowsAzure.Storage.Table;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public class TransactionTask : IndexTableEntitiesTaskBase<TransactionEntry.Entity>
    {
        public TransactionTask(AzureStorageClient storageClient)
            : base(storageClient)
        {
        }
        
        protected override void ProcessBlock(BlockInfo blockInfo, BulkImport<TransactionEntry.Entity> bulk)
        {
            foreach (var transaction in blockInfo.Block.Transactions)
            {
                var indexed = new TransactionEntry.Entity(null, transaction, blockInfo.BlockId);
                bulk.Add(indexed.PartitionKey, indexed);
            }
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetTransactionTable();
        }

        protected override ITableEntity ToTableEntity(TransactionEntry.Entity indexed)
        {
            return indexed.CreateTableEntity();
        }
    }
}
