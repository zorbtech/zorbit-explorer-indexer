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
    public sealed class AddressSummaryAdapter : TableEntityAdapter<AddressSummaryModel>, IAddressSummary, ITaskAdapter
    {
        private const string KindKey = "kind";
        private const string BalanceKey = "balance";
        private const string SentKey = "sent";
        private const string ReceivedKey = "received";
        private const string StakedKey = "staked";
        private const string TxCountKey = "txcount";

        public AddressSummaryAdapter()
        {
        }

        public AddressSummaryAdapter(AddressSummaryModel originalEntity)
            : base(originalEntity,
                originalEntity.Address.Partition(), 
                $"{originalEntity.Address}-{originalEntity.Kind.ToString().ToLowerInvariant()}")
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
        public Money Balance
        {
            get => OriginalEntity.Balance;
            set => OriginalEntity.Balance = value;
        }

        public Money Sent
        {
            get => OriginalEntity.Sent;
            set => OriginalEntity.Sent = value;
        }

        public Money Received
        {
            get => OriginalEntity.Received;
            set => OriginalEntity.Received = value;
        }

        public Money Staked
        {
            get => OriginalEntity.Staked;
            set => OriginalEntity.Staked = value;
        }

        public int TxCount
        {
            get => OriginalEntity.TxCount;
            set => OriginalEntity.TxCount = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new AddressSummaryModel
            {
                Address = RowKey.Substring(0, RowKey.IndexOf('-')),
                Kind = AddressKind.Summary,
                Balance = Money.Satoshis(properties[BalanceKey].Int64Value.Value),
                Sent = Money.Satoshis(properties[SentKey].Int64Value.Value),
                Received = Money.Satoshis(properties[ReceivedKey].Int64Value.Value),
                Staked = Money.Satoshis(properties[StakedKey].Int64Value.Value),
                TxCount = properties[TxCountKey].Int32Value.Value
            };
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>
            {
                { KindKey, new EntityProperty(Kind.ToString().ToLowerInvariant()) },
                { BalanceKey, new EntityProperty(Balance.Satoshi) },
                { SentKey, new EntityProperty(Sent.Satoshi) },
                { ReceivedKey, new EntityProperty(Received.Satoshi) },
                { StakedKey, new EntityProperty(Staked.Satoshi) },
                { TxCountKey, new EntityProperty(TxCount) }
            };
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}
