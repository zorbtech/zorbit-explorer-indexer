using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Entities;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public class BlockSummaryTask : IndexTableEntitiesTaskBase<BlockSummaryEntity>
    {
        private readonly string _partitionKey;

        public BlockSummaryTask(
            string partitionKey,
            AzureStorageClient storageClient)
            : base(storageClient)
        {
            _partitionKey = partitionKey;
        }

        protected override void ProcessBlock(BlockInfo blockInfo, BulkImport<BlockSummaryEntity> bulk)
        {
            bulk.Add(_partitionKey, new BlockSummaryEntity(_partitionKey, blockInfo));
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetBlockSummaryTable();
        }

        protected override ITableEntity ToTableEntity(BlockSummaryEntity item)
        {
            return item.ToEntity();
        }
    }
}
