using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Indexing;
using Zorbit.Features.Observatory.TableStorage.Utils;

namespace Zorbit.Features.Observatory.TableStorage.Adapters
{
    public class CheckpointAdapter : TableEntityAdapter<CheckpointModel>, ICheckpoint, ITaskAdapter
    {
        public CheckpointAdapter()
        {
        }
        
        public CheckpointAdapter(CheckpointModel originalEntity)
            : base(
                originalEntity,
                originalEntity.IndexType.ToString().ToLowerInvariant(),
                string.Empty)
        {
        }

        public IndexType IndexType
        {
            get => OriginalEntity.IndexType;
            set => OriginalEntity.IndexType = value;
        }

        public BlockLocator BlockLocator
        {
            get => OriginalEntity.BlockLocator;
            set => OriginalEntity.BlockLocator = value;
        }

        public uint256 Genesis => OriginalEntity.Genesis;

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new CheckpointModel
            {
                IndexType = (IndexType)Enum.Parse(typeof(IndexType), PartitionKey, true)
            };
            OriginalEntity.BlockLocator.Blocks.AddRange(properties.Values.Select(hash => new uint256(hash.StringValue)));
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return BlockLocator.Blocks
                .Select((hash, index) => new { index, hash })
                .ToDictionary(kp => $"block{kp.index}", kp => new EntityProperty(kp.hash.ToString()));
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}
