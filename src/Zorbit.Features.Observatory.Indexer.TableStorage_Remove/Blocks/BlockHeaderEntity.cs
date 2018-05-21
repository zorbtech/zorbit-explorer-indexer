using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Zorbit.Features.AzureIndexer.TableStorage;

namespace Stratis.Bitcoin.Features.AzureIndexer.TableStorage.Entities
{
    public class BlockHeaderEntity : AzureTableEntity
    {
        public int Height { get; set; }

        public BlockHeader BlockHeader { get; set; }

        public BlockHeaderEntity(string partitionKey)
            : base(partitionKey)
        {
        }

        public BlockHeaderEntity(string partitionKey, DynamicTableEntity entity)
            : base(partitionKey)
        {
            Height = entity.RowKey;

            var headerProperty = entity.Properties.First();
            var header = new BlockHeader();
            header.FromBytes(headerProperty.Value.BinaryValue);
            BlockHeader = header;
        }

        public override ITableEntity ToEntity()
        {
            var entity = new DynamicTableEntity
            {
                PartitionKey = PartitionKey,
                RowKey = Helper.HeightToString(Height)
            };

            entity.Properties.Add("header", new EntityProperty(BlockHeader.ToBytes()));

            return entity;
        }
    }
}
