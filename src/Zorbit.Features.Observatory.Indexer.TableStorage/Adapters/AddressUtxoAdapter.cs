using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Indexing;
using Zorbit.Features.Observatory.TableStorage.Utils;

namespace Zorbit.Features.Observatory.TableStorage.Adapters
{
    public sealed class AddressUtxoAdapter : TableEntityAdapter<AddressUtxoModel>, IAddressUtxo, ITaskAdapter
    {
        private const string KindKey = "kind";
        private const string TxBlockHeightKey = "txblockheight";
        private const string TxIndexKey = "txindex";
        private const string TxValueKey = "txvalue";
        private const string TxSpentHeightKey = "txspentheight";
        private const string TxSpentKey = "txspent";

        public AddressUtxoAdapter()
        {
        }

        public AddressUtxoAdapter(AddressUtxoModel originalEntity)
            : base(originalEntity, originalEntity.Address, string.Empty)
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

        public int BlockHeight
        {
            get => OriginalEntity.BlockHeight;
            set => OriginalEntity.BlockHeight = value;
        }

        public int TxIndex
        {
            get => OriginalEntity.TxIndex;
            set => OriginalEntity.TxIndex = value;
        }

        public Money Value
        {
            get => OriginalEntity.Value;
            set => OriginalEntity.Value = value;
        }

        public uint256 TxSpentId
        {
            get => OriginalEntity.TxSpentId;
            set => OriginalEntity.TxSpentId = value;
        }

        public int TxSpentHeight
        {
            get => OriginalEntity.TxSpentHeight;
            set => OriginalEntity.TxSpentHeight = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new AddressUtxoModel
            {
                Address = PartitionKey,
                Kind = AddressKind.Utxo,
                BlockHeight = properties[TxBlockHeightKey].Int32Value.Value,
                TxIndex = properties[TxIndexKey].Int32Value.Value,
                Value = Money.Satoshis(properties[TxValueKey].Int64Value.Value),
                TxSpentHeight = properties[TxSpentHeightKey].Int32Value.Value,
            };

            var txSpent = properties[TxSpentKey].StringValue;
            if (!string.IsNullOrEmpty(txSpent))
            {
                OriginalEntity.TxSpentId = uint256.Parse(txSpent);
            }
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>
            {
                { KindKey, new EntityProperty(Kind.ToString().ToLowerInvariant()) },
                { TxBlockHeightKey, new EntityProperty(BlockHeight) },
                { TxIndexKey, new EntityProperty(TxIndex) },
                { TxValueKey, new EntityProperty(Value.Satoshi) },
                { TxSpentKey, new EntityProperty(TxSpentId?.ToString()) },
                { TxSpentHeightKey, new EntityProperty(TxSpentHeight) },
            };
        }

        public int GetSize()
        {
            return this.GetAdapterSize();
        }
    }
}
