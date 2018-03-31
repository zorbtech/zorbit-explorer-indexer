using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public class ChainPartEntry
    {
        public ChainPartEntry()
        {
            BlockHeaders = new List<BlockHeader>();
        }

        public ChainPartEntry(DynamicTableEntity entity)
        {
            ChainOffset = Helper.StringToHeight(entity.RowKey);
            BlockHeaders = new List<BlockHeader>();

            foreach (var prop in entity.Properties)
            {
                var header = new BlockHeader();
                header.FromBytes(prop.Value.BinaryValue);
                BlockHeaders.Add(header);
            }
        }

        public BlockHeader GetHeader(int height)
        {
            if (height < ChainOffset)
            {
                return null;
            }

            height = height - ChainOffset;
            return height >= BlockHeaders.Count ? null : BlockHeaders[height];
        }

        public DynamicTableEntity ToEntity()
        {
            var entity = new DynamicTableEntity
            {
                PartitionKey = "a",
                RowKey = Helper.HeightToString(ChainOffset)
            };

            var i = 0;
            foreach (var header in BlockHeaders)
            {
                entity.Properties.Add($"a{i}", new EntityProperty(header.ToBytes()));
                i++;
            }

            return entity;
        }

        public int ChainOffset { get; set; }

        public List<BlockHeader> BlockHeaders { get; }
    }
}
