using System;
using System.Linq;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IBlockSummary
    {
        int Height { get; set; }
        uint256 Hash { get; set; }
        BlockHeader Header { get; set; }
        bool PosBlock { get; set; }
        int Size { get; set; }
        DateTimeOffset Time { get; set; }
        int TxCount { get; set; }
        Money TxTotal { get; set; }
    }

    public sealed class BlockSummaryModel : IBlockSummary
    {
        public BlockSummaryModel()
        {
        }

        public BlockSummaryModel(IBlockInfo blockInfo)
        {
            Height = blockInfo.Height;
            Hash = blockInfo.Hash;
            Header = blockInfo.Block.Header;
            TxCount = blockInfo.Block.Transactions.Count;
            TxTotal = blockInfo.Block.Transactions.Sum(tx => tx.TotalOut);
            Size = blockInfo.Block.GetSerializedSize();
            Time = Utils.UnixTimeToDateTime(blockInfo.Block.Header.Time);
            PosBlock = blockInfo.Block.Transactions.Any(tx => tx.IsCoinStake);
        }

        public int Height { get; set; }
        public uint256 Hash { get; set; }
        public BlockHeader Header { get; set; }
        public bool PosBlock { get; set; }
        public int Size { get; set; }
        public DateTimeOffset Time { get; set; }
        public int TxCount { get; set; }
        public Money TxTotal { get; set; }
    }
}