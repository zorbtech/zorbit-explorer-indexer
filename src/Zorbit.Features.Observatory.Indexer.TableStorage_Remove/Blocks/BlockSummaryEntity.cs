using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Zorbit.Features.AzureIndexer.TableStorage;
using Zorbit.Features.Indexer.Core.Blocks;

namespace Stratis.Bitcoin.Features.AzureIndexer.TableStorage.Entities
{
    public class BlockSummaryEntity : AzureTableEntity
    {
        public int Height { get; set; }

        public uint256 Hash { get; set; }

        public DateTimeOffset Time { get; set; }

        public int TxCount { get; set; }

        public Money TxTotal { get; set; }

        public int Size { get; set; }

        public bool PosBlock { get; set; }

        public BlockSummaryEntity(string partitionKey, IBlockInfo block)
            : base(partitionKey)
        {
            Height = block.Height;
            Hash = block.BlockId;
            TxCount = block.Block.Transactions.Count;
            TxTotal = block.Block.Transactions.Sum(tx => tx.TotalOut);
            Size = block.Block.GetSerializedSize();
            Time = NBitcoin.Utils.UnixTimeToDateTime(block.Block.Header.Time);
            PosBlock = block.Block.Transactions.Any(tx => tx.IsCoinStake);
        }

        public override ITableEntity ToEntity()
        {
            var entity = new DynamicTableEntity
            {
                PartitionKey = PartitionKey,
                RowKey = Helper.HeightToString(Height)
            };

            entity.Properties.Add("hash", new EntityProperty(Hash.ToString()));
            entity.Properties.Add("time", new EntityProperty(Time));
            entity.Properties.Add("txcount", new EntityProperty(TxCount));
            entity.Properties.Add("txtotal", new EntityProperty(TxTotal.Satoshi));
            entity.Properties.Add("size", new EntityProperty(Size));
            entity.Properties.Add("pos", new EntityProperty(PosBlock));

            return entity;
        }
    }
}
