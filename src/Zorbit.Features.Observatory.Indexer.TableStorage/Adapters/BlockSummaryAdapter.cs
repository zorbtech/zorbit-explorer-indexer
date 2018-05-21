using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin.Utilities.Extensions;
using Zorbit.Features.Observatory.Core.Extensions;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Indexing;
using Zorbit.Features.Observatory.TableStorage.Utils;

namespace Zorbit.Features.Observatory.TableStorage.Adapters
{
    public sealed class BlockSummaryAdapter : TableEntityAdapter<BlockSummaryModel>, IBlockSummary, ITaskAdapter
    {
        private const string HeightKey = "height";
        private const string HashKey = "hash";
        private const string HeaderKey = "header";
        private const string PosKey = "pos";
        private const string TxCountKey = "txcount";
        private const string TxTotalKey = "txtotal";
        private const string SizeKey = "size";
        private const string TimeKey = "time";

        public BlockSummaryAdapter()
        {
        }

        public BlockSummaryAdapter(BlockSummaryModel originalEntity)
            : base(
                originalEntity,
                originalEntity.Time.Date.ToUnixTimestamp().ToDecendingString(),
                originalEntity.Height.ToDecendingString())
        {
        }

        public int Height
        {
            get => OriginalEntity.Height;
            set => OriginalEntity.Height = value;
        }

        public uint256 Hash
        {
            get => OriginalEntity.Hash;
            set => OriginalEntity.Hash = value;
        }

        public BlockHeader Header
        {
            get => OriginalEntity.Header;
            set => OriginalEntity.Header = value;
        }

        public bool PosBlock
        {
            get => OriginalEntity.PosBlock;
            set => OriginalEntity.PosBlock = value;
        }

        public int Size
        {
            get => OriginalEntity.Size;
            set => OriginalEntity.Size = value;
        }

        public DateTimeOffset Time
        {
            get => OriginalEntity.Time;
            set => OriginalEntity.Time = value;
        }

        public int TxCount
        {
            get => OriginalEntity.TxCount;
            set => OriginalEntity.TxCount = value;
        }

        public Money TxTotal
        {
            get => OriginalEntity.TxTotal;
            set => OriginalEntity.TxTotal = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new BlockSummaryModel
            {
                Height = properties[HeightKey].Int32Value.Value,
                Hash = uint256.Parse(properties[HashKey].StringValue),
                Header = BlockHeader.Parse(properties[HeaderKey].StringValue),
                PosBlock = properties[PosKey].BooleanValue.Value,
                TxCount = properties[TxCountKey].Int32Value.Value,
                TxTotal = Money.Satoshis(properties[TxTotalKey].Int64Value.Value),
                Size = properties[SizeKey].Int32Value.Value,
                Time = properties[TimeKey].DateTimeOffsetValue.Value
            };
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>
            {
                { HeightKey, new EntityProperty(Height) },
                { HashKey, new EntityProperty(Hash.ToString()) },
                { HeaderKey, new EntityProperty(Header.ToBytes()) },
                { PosKey, new EntityProperty(PosBlock) },
                { TxCountKey, new EntityProperty(TxCount) },
                { TxTotalKey, new EntityProperty(TxTotal.Satoshi) },
                { SizeKey, new EntityProperty(Size) },
                { TimeKey, new EntityProperty(Time) }
            };
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}