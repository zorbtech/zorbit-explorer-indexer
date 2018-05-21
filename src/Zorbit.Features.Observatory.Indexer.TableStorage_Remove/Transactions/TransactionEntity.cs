using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using NBitcoin.OpenAsset;

namespace Zorbit.Features.AzureIndexer.TableStorage.Transactions
{
    public enum TransactionEntryType
    {
        Mempool,
        ConfirmedTransaction,
        Colored
    }

    public class TransactionEntity : AzureTableEntity
    {
        public uint256 BlockId { get; set; }

        public uint256 TxId { get; set; }

        public ColoredTransaction ColoredTransaction { get; set; }

        public Transaction Transaction { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public List<TxOut> PreviousTxOuts { get; } = new List<TxOut>();

        public TransactionEntryType Type { get; set; }

        public bool IsLoaded => Transaction != null && (Transaction.IsCoinBase || (PreviousTxOuts.Count == Transaction.Inputs.Count));

        public TransactionEntity(string partitionKey, uint256 txId) : base(partitionKey)
        {
            Timestamp = DateTimeOffset.UtcNow;
            TxId = txId ?? throw new ArgumentNullException("txId");
        }

        public TransactionEntity(string partitionKey, uint256 txId, ColoredTransaction colored) : base(partitionKey)
        {
            TxId = txId ?? throw new ArgumentNullException("txId");
            ColoredTransaction = colored;
            Type = TransactionEntryType.Colored;
        }

        public TransactionEntity(string partitionKey, Transaction tx, uint256 blockId) : base(partitionKey)
        {
            Timestamp = DateTimeOffset.UtcNow;
            TxId = tx.GetHash();
            Transaction = tx;
            BlockId = blockId;
            Type = blockId == null ?
                TransactionEntryType.Mempool :
                TransactionEntryType.ConfirmedTransaction;
        }

        public TransactionEntity(DynamicTableEntity entity) : base(entity.PartitionKey)
        {
            var splitted = entity.RowKey.Split(new[] { "-" }, StringSplitOptions.None);

            Timestamp = entity.Timestamp;
            TxId = uint256.Parse(splitted[0]);
            Type = GetType(splitted[1]);

            if (splitted.Length >= 3 && splitted[2] != string.Empty)
            {
                BlockId = uint256.Parse(splitted[2]);
            }

            var bytes = Helper.GetEntityProperty(entity, "tx");
            if (bytes != null && bytes.Length != 0)
            {
                Transaction = new Transaction();
                Transaction.ReadWrite(bytes);
            }

            bytes = Helper.GetEntityProperty(entity, "ctx");
            if (bytes != null && bytes.Length != 0)
            {
                ColoredTransaction = new ColoredTransaction();
                ColoredTransaction.ReadWrite(bytes);
            }

            PreviousTxOuts = Helper.DeserializeList<TxOut>(Helper.GetEntityProperty(entity, "previoustxouts"));

            var timestamp = Helper.GetEntityProperty(entity, "time");
            if (timestamp != null && timestamp.Length == 8)
            {
                Timestamp = new DateTimeOffset((long)ToUInt64(timestamp, 0), TimeSpan.Zero);
            }
        }

        public static ulong ToUInt64(byte[] value, int index)
        {
            return value[index]
                   + ((ulong)value[index + 1] << 8)
                   + ((ulong)value[index + 2] << 16)
                   + ((ulong)value[index + 3] << 24)
                   + ((ulong)value[index + 4] << 32)
                   + ((ulong)value[index + 5] << 40)
                   + ((ulong)value[index + 6] << 48)
                   + ((ulong)value[index + 7] << 56);
        }

        public override ITableEntity ToEntity()
        {
            var entity = new DynamicTableEntity
            {
                PartitionKey = PartitionKey,
                RowKey = $"{TxId}-{BlockId}"
            };

            if (Transaction != null)
            {
                Helper.SetEntityProperty(entity, "tx", Transaction.ToBytes());
            }

            if (ColoredTransaction != null)
            {
                Helper.SetEntityProperty(entity, "ctx", ColoredTransaction.ToBytes());
            }

            Helper.SetEntityProperty(entity, "previoustxouts", Helper.SerializeList(PreviousTxOuts));
            Helper.SetEntityProperty(entity, "time", NBitcoin.Utils.ToBytes((ulong)Timestamp.UtcTicks, true));

            return entity;
        }

        public TransactionEntryType GetType(string letter)
        {
            switch (letter[0])
            {
                case 'c':
                    return TransactionEntryType.Colored;
                case 'b':
                    return TransactionEntryType.ConfirmedTransaction;
                case 'm':
                    return TransactionEntryType.Mempool;
                default:
                    return TransactionEntryType.Mempool;
            }
        }
    }
}
