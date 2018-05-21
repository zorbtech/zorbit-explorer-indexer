using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Zorbit.Features.Observatory.Core.Extensions;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Indexing;
using Zorbit.Features.Observatory.TableStorage.Utils;

namespace Zorbit.Features.Observatory.TableStorage.Adapters
{
    public sealed class BlockChunkAdapter : TableEntityAdapter<BlockChunkModel>, IBlockChunk, ITaskAdapter
    {
        private const string HashKey = "hash";
        private const string HeightKey = "height";
        private const string IndexKey = "index";
        private const string ChunkKey = "chunk";

        public BlockChunkAdapter()
        {
        }

        public BlockChunkAdapter(BlockChunkModel originalEntity)
            : base(
                originalEntity,
                $"block-{originalEntity.Hash.ToString().Partition()}",
                $"{originalEntity.Hash}-chunk-{originalEntity.Index:D10}")
        {
        }

        public uint256 Hash
        {
            get => OriginalEntity.Hash;
            set => OriginalEntity.Hash = value;
        }

        public int Index
        {
            get => OriginalEntity.Index;
            set => OriginalEntity.Index = value;
        }

        public int Height
        {
            get => OriginalEntity.Height;
            set => OriginalEntity.Height = value;
        }

        public IList<byte[]> Chunks
        {
            get => OriginalEntity.Chunks;
            set => OriginalEntity.Chunks = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new BlockChunkModel
            {
                Hash = uint256.Parse(properties[HashKey].StringValue),
                Height = properties[HeightKey].Int32Value.Value,
                Index = properties[IndexKey].Int32Value.Value,
                Chunks = properties.GetProperty(ChunkKey)
            };
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var result = new Dictionary<string, EntityProperty>
            {
                { HashKey, new EntityProperty(Hash.ToString()) },
                { HeightKey, new EntityProperty(Height) },
                { IndexKey, new EntityProperty(Index) }
            };

            foreach (var kp in Chunks.GetProperties(ChunkKey))
            {
                result.Add(kp.Key, kp.Value);
            }

            return result;
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}
