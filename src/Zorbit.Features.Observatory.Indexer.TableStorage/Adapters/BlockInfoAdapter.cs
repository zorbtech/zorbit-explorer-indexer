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
    public sealed class BlockInfoAdapter : TableEntityAdapter<BlockInfoModel>, IBlockInfo, ITaskAdapter
    {
        private const string HashKey = "hash";
        private const string HeightKey = "height";

        public BlockInfoAdapter()
        {
        }

        public BlockInfoAdapter(BlockInfoModel originalEntity)
            : base(
                originalEntity,
                $"block-{originalEntity.Hash.ToString().Partition()}",
                $"{originalEntity.Hash}")
        {
        }

        public uint256 Hash
        {
            get => OriginalEntity.Hash;
            set => OriginalEntity.Hash = value;
        }

        public int Height
        {
            get => OriginalEntity.Height;
            set => OriginalEntity.Height = value;
        }

        public Block Block
        {
            get => OriginalEntity.Block;
            set => OriginalEntity.Block = value;
        }

        public IEnumerable<IBlockChunk> GetChunks()
        {
            return OriginalEntity.GetChunks();
        }

        public void ParseChunks(IEnumerable<IBlockChunk> chunks)
        {
            OriginalEntity.ParseChunks(chunks);
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new BlockInfoModel
            {
                Hash = uint256.Parse(properties[HashKey].StringValue),
                Height = properties[HeightKey].Int32Value.Value
            };
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>
            {
                { HashKey, new EntityProperty(Hash.ToString()) },
                { HeightKey, new EntityProperty(Height) }
            };
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}
