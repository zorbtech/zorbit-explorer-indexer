using System;
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
    public sealed class AddressTransactionAdapter : TableEntityAdapter<AddressTransactionModel>, IAddressTransaction, ITaskAdapter
    {
        private const string KindKey = "kind";
        private const string TxIdKey = "txid";
        private const string TxBlockIdKey = "txblockid";
        private const string TxBlockHeightKey = "txblockheight";
        private const string TxTimeKey = "txtime";
        private const string TxValueKey = "txvalue";
        private const string TxTypeKey = "txtype";
        private const string TxBalanceKey = "txbalance";

        public AddressTransactionAdapter()
        {
        }

        public AddressTransactionAdapter(AddressTransactionModel originalEntity)
            : base(originalEntity,
                originalEntity.Address.Partition(),
                $"{originalEntity.Address}-{originalEntity.Kind.ToString().ToLowerInvariant()}-{originalEntity.BlockHeight.ToDecendingString()}-{originalEntity.TxId}")
        {
        }

        public string Address
        {
            get => OriginalEntity.Address;
            set => OriginalEntity.Address = value;
        }

        public AddressKind Kind
        {
            get => OriginalEntity.Kind;
            set => OriginalEntity.Kind = value;
        }

        public uint256 TxId
        {
            get => OriginalEntity.TxId;
            set => OriginalEntity.TxId = value;
        }

        public uint256 BlockId
        {
            get => OriginalEntity.BlockId;
            set => OriginalEntity.BlockId = value;
        }

        public int BlockHeight
        {
            get => OriginalEntity.BlockHeight;
            set => OriginalEntity.BlockHeight = value;
        }

        public DateTimeOffset Time
        {
            get => OriginalEntity.Time;
            set => OriginalEntity.Time = value;
        }

        public Money Value
        {
            get => OriginalEntity.Value;
            set => OriginalEntity.Value = value;
        }

        public TransactionType TxType
        {
            get => OriginalEntity.TxType;
            set => OriginalEntity.TxType = value;
        }

        public Money TxBalance
        {
            get => OriginalEntity.TxBalance;
            set => OriginalEntity.TxBalance = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new AddressTransactionModel
            {
                Address = RowKey.Substring(0, RowKey.IndexOf('-')),
                Kind = AddressKind.Transaction,
                TxId = uint256.Parse(properties[TxIdKey].StringValue),
                BlockId = uint256.Parse(properties[TxBlockIdKey].StringValue),
                BlockHeight = properties[TxBlockHeightKey].Int32Value.Value,
                Time = properties[TxTimeKey].DateTimeOffsetValue.Value,
                Value = Money.Satoshis(properties[TxValueKey].Int64Value.Value),
                TxType = (TransactionType)Enum.Parse(typeof(Transaction), properties[TxTypeKey].ToString(), true),
                TxBalance = Money.Satoshis(properties[TxBalanceKey].Int64Value.Value)
            };
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>
            {
                { KindKey, new EntityProperty(Kind.ToString().ToLowerInvariant()) },
                { TxIdKey, new EntityProperty(TxId.ToString()) },
                { TxBlockIdKey, new EntityProperty(BlockId.ToString()) },
                { TxBlockHeightKey, new EntityProperty(BlockHeight) },
                { TxTimeKey, new EntityProperty(Time) },
                { TxValueKey, new EntityProperty(Value.Satoshi) },
                { TxTypeKey, new EntityProperty(TxType.ToString().ToLowerInvariant()) },
                { TxBalanceKey, new EntityProperty(TxBalance.Satoshi) }
            };
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}
