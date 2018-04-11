using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;
using Stratis.Bitcoin.Features.AzureIndexer.Entities;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public sealed class BlockTask : IndexTableEntitiesTaskBase<BlockTableEntity>
    {
        private readonly string _partitionKey;

        public BlockTask(
            string partitionKey,
            AzureStorageClient storageClient)
            : base(storageClient)
        {
            _partitionKey = partitionKey;
        }

        protected override void ProcessBlock(BlockInfo blockInfo, BulkImport<BlockTableEntity> bulk)
        {
            var entries = BlockTableEntity.GetBlockEntries(_partitionKey, blockInfo);
            foreach (var entry in entries)
            {
                bulk.Add(_partitionKey, entry);
            }
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetBlockTable();
        }

        protected override ITableEntity ToTableEntity(BlockTableEntity item)
        {
            return item.ToEntity();
        }
    }
}
