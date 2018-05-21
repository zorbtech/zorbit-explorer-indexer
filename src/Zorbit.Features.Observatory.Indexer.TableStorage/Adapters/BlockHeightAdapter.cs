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
    public sealed class BlockHeightAdapter : TableEntityAdapter<BlockHeightModel>, IBlockHeight, ITaskAdapter
    {
        private const string HashKey = "hash";
        private const string HeightKey = "height";

        public BlockHeightAdapter()
        {
        }

        public BlockHeightAdapter(BlockHeightModel originalEntity)
            : base(
                originalEntity,
                $"height-{originalEntity.Height.Partition()}",
                $"{originalEntity.Height.ToDecendingString()}")
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

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new BlockHeightModel
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
