using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public class ChainPartEntity
    {
        public ChainPartEntity()
        {
        }

        public ChainPartEntity(DynamicTableEntity entity)
        {
            Height = Helper.StringToHeight(entity.RowKey);

            var headerProperty = entity.Properties.First();
            var header = new BlockHeader();
            header.FromBytes(headerProperty.Value.BinaryValue);
            BlockHeader = header;
        }

        public DynamicTableEntity ToEntity()
        {
            var entity = new DynamicTableEntity
            {
                PartitionKey = "a",
                RowKey = Helper.HeightToString(Height)
            };

            entity.Properties.Add($"a0", new EntityProperty(BlockHeader.ToBytes()));

            return entity;
        }

        public int Height { get; set; }

        public BlockHeader BlockHeader { get; set; }
    }
}
