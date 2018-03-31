﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using NBitcoin.OpenAsset;
using Stratis.Bitcoin.Features.AzureIndexer.Utils;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public class TransactionEntry
    {
        public class Entity
        {
            private string _partitionKey;

            public enum TransactionEntryType
            {
                Mempool,
                ConfirmedTransaction,
                Colored
            }

            public Entity(uint256 txId)
            {
                Timestamp = DateTimeOffset.UtcNow;
                TxId = txId ?? throw new ArgumentNullException("txId");
            }

            public Entity(DynamicTableEntity entity)
            {
                var splitted = entity.RowKey.Split(new string[] { "-" }, StringSplitOptions.None);
                _partitionKey = entity.PartitionKey;
                Timestamp = entity.Timestamp;
                TxId = uint256.Parse(splitted[0]);
                Type = GetType(splitted[1]);

                if (splitted.Length >= 3 && splitted[2] != string.Empty)
                {
                    BlockId = uint256.Parse(splitted[2]);
                }

                var bytes = Helper.GetEntityProperty(entity, "a");
                if (bytes != null && bytes.Length != 0)
                {
                    Transaction = new Transaction();
                    Transaction.ReadWrite(bytes);
                }

                bytes = Helper.GetEntityProperty(entity, "b");
                if (bytes != null && bytes.Length != 0)
                {
                    ColoredTransaction = new ColoredTransaction();
                    ColoredTransaction.ReadWrite(bytes);
                }

                PreviousTxOuts = Helper.DeserializeList<TxOut>(Helper.GetEntityProperty(entity, "c"));

                var timestamp = Helper.GetEntityProperty(entity, "d");
                if (timestamp != null && timestamp.Length == 8)
                {
                    Timestamp = new DateTimeOffset((long)ToUInt64(timestamp, 0), TimeSpan.Zero);
                }
            }

            public Entity(uint256 txId, ColoredTransaction colored)
            {
                TxId = txId ?? throw new ArgumentNullException("txId");
                ColoredTransaction = colored;
                Type = TransactionEntryType.Colored;
            }

            public Entity(uint256 txId, Transaction tx, uint256 blockId)
            {
                if (txId == null)
                {
                    txId = tx.GetHash();
                }

                Timestamp = DateTimeOffset.UtcNow;
                TxId = txId;
                Transaction = tx;
                BlockId = blockId;
                Type = blockId == null ?
                    TransactionEntryType.Mempool :
                    TransactionEntryType.ConfirmedTransaction;
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

            public DynamicTableEntity CreateTableEntity()
            {
                var entity = new DynamicTableEntity
                {
                    ETag = "*",
                    PartitionKey = PartitionKey,
                    RowKey = $"{TxId}-{TypeLetter}-{BlockId}"
                };

                if (Transaction != null)
                {
                    Helper.SetEntityProperty(entity, "a", Transaction.ToBytes());
                }

                if (ColoredTransaction != null)
                {
                    Helper.SetEntityProperty(entity, "b", ColoredTransaction.ToBytes());
                }

                Helper.SetEntityProperty(entity, "c", Helper.SerializeList(PreviousTxOuts));
                Helper.SetEntityProperty(entity, "d", NBitcoin.Utils.ToBytes((ulong)Timestamp.UtcTicks, true));

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

            public string PartitionKey
            {
                get
                {
                    if (_partitionKey != null || TxId == null)
                    {
                        return _partitionKey;
                    }

                    var b = TxId.ToBytes();
                    _partitionKey = Helper.GetPartitionKey(10, new[] { b[0], b[1] }, 0, 2);
                    return _partitionKey;
                }
            }

            public string TypeLetter => Type == TransactionEntryType.Colored ? "c" :
                Type == TransactionEntryType.ConfirmedTransaction ? "b" :
                Type == TransactionEntryType.Mempool ? "m" : "?";

            public DateTimeOffset Timestamp { get; set; }

            public bool IsLoaded => Transaction != null && (Transaction.IsCoinBase || (PreviousTxOuts.Count == Transaction.Inputs.Count));

            public uint256 BlockId { get; set; }

            public uint256 TxId { get; set; }

            public ColoredTransaction ColoredTransaction { get; set; }

            public Transaction Transaction { get; set; }

            public List<TxOut> PreviousTxOuts { get; } = new List<TxOut>();

            public TransactionEntryType Type { get; set; }
        }

        internal TransactionEntry(IReadOnlyList<Entity> entities)
        {
            Guard.NotNull(entities, "entities");

            TransactionId = entities[0].TxId;
            BlockIds = entities.Select(e => e.BlockId).Where(b => b != null).ToArray();

            MempoolDate = entities.Where(e => e.Type == Entity.TransactionEntryType.Mempool)
                                  .Select(e => new DateTimeOffset?(e.Timestamp))
                                  .Min();

            FirstSeen = MempoolDate ?? entities.Where(e => e.Type == Entity.TransactionEntryType.ConfirmedTransaction)
                            .Select(e => new DateTimeOffset?(e.Timestamp))
                            .Min().Value;

            var loadedEntity = entities.FirstOrDefault(e => e.Transaction != null && e.IsLoaded) ?? 
                               entities.FirstOrDefault(e => e.Transaction != null);

            if (loadedEntity != null)
            {
                Transaction = loadedEntity.Transaction;
                if (loadedEntity.Transaction.IsCoinBase)
                {
                    SpentCoins = new List<Spendable>();
                }
                else if (loadedEntity.IsLoaded)
                {
                    SpentCoins = new List<Spendable>();
                    for (var i = 0; i < Transaction.Inputs.Count; i++)
                    {
                        SpentCoins.Add(new Spendable(Transaction.Inputs[i].PrevOut, loadedEntity.PreviousTxOuts[i]));
                    }
                }
            }

            var coloredLoadedEntity = entities.FirstOrDefault(e => e.ColoredTransaction != null);
            if (coloredLoadedEntity != null)
            {
                ColoredTransaction = coloredLoadedEntity.ColoredTransaction;
            }
        }

        public Money Fees
        {
            get
            {
                if (SpentCoins == null || Transaction == null)
                {
                    return null;
                }

                if (Transaction.IsCoinBase)
                {
                    return Money.Zero;
                }

                return SpentCoins.Select(o => o.TxOut.Value).Sum() - Transaction.TotalOut;
            }
        }

        public uint256[] BlockIds { get; internal set; }

        public uint256 TransactionId { get; internal set; }

        public Transaction Transaction { get; internal set; }

        public List<Spendable> SpentCoins { get; set; }

        public DateTimeOffset FirstSeen { get; set; }

        public ColoredTransaction ColoredTransaction { get; set; }

        public DateTimeOffset? MempoolDate { get; set; }
    }
}
