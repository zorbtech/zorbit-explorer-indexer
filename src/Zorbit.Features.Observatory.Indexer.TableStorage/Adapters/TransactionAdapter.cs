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
    public class TransactionAdapter : TableEntityAdapter<TransactionModel>, ITransaction, ITaskAdapter
    {
        private const string BlockIdKey = "blockId";
        private const string TxKey = "tx";

        public TransactionAdapter()
        {
        }

        public TransactionAdapter(TransactionModel originalEntity)
            : base(
                originalEntity,
                originalEntity.Transaction.GetHash().ToString().Partition(),
                originalEntity.Transaction.GetHash().ToString())
        {
        }

        public uint256 BlockId
        {
            get => OriginalEntity.BlockId;
            set => OriginalEntity.BlockId = value;
        }

        public Transaction Transaction
        {
            get => OriginalEntity.Transaction;
            set => OriginalEntity.Transaction = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            OriginalEntity = new TransactionModel
            {
                BlockId = uint256.Parse(properties[BlockIdKey].StringValue),
                Transaction = new Transaction(properties.PropertiesToByteArray(TxKey))
            };
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var result = new Dictionary<string, EntityProperty>
            {
                { BlockIdKey, new EntityProperty(BlockId.ToString()) }
            };

            foreach (var kp in Transaction.ToBytes().ByteArrayToProperties(TxKey))
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
