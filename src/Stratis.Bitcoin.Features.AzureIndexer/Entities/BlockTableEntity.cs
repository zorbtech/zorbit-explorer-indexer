using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Entities
{
    public class BlockTableEntity : AzureTableEntity
    {
        public uint256 Hash { get; set; }

        public int Index { get; set; }

        public List<byte[]> Chunks { get; } = new List<byte[]>();

        private BlockTableEntity(string partitionKey) : base(partitionKey)
        {
        }

        public static IEnumerable<BlockTableEntity> GetBlockEntries(string partitionKey, BlockInfo blockInfo)
        {
            var results = new List<BlockTableEntity>();

            var block = blockInfo.Block.ToBytes();
            var parts = 0;
            var index = 0;

            var b = new BlockTableEntity(partitionKey)
            {
                Hash = blockInfo.BlockId,
                Index = index
            };

            results.Add(b);

            foreach (var part in block.Split(64))
            {
                b.Chunks.Add(part.ToArray());
                parts++;

                if (parts != 200)
                {
                    continue;
                }

                parts = 0;
                index++;
                b = new BlockTableEntity(partitionKey)
                {
                    Hash = blockInfo.BlockId,
                    Index = index
                };
                results.Add(b);
            }

            return results;
        }

        public static Block GetBlock(IEnumerable<BlockTableEntity> entries)
        {
            IEnumerable<byte> bytes = new List<byte>();
            bytes = entries.SelectMany(entry => entry.Chunks)
                .Aggregate(bytes, (current, chunk) => current.Concat(chunk));
            return new Block(bytes.ToArray());
        }

        public DynamicTableEntity ToEntity()
        {
            var entity = new DynamicTableEntity
            {
                PartitionKey = PartitionKey,
                RowKey = $"{Hash}-{Index}"
            };

            entity.Properties.Add("index", new EntityProperty(Index));

            for (var i = 0; i < Chunks.Count; i++)
            {
                entity.Properties.Add($"chunk{i}", new EntityProperty(Chunks[i]));
            }

            return entity;
        }
    }
}
