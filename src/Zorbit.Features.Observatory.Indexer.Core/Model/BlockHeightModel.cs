using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface IBlockHeight
    {
        uint256 Hash { get; set; }
        int Height { get; set; }
    }


    public sealed class BlockHeightModel : IBlockHeight
    {
        public uint256 Hash { get; set; }
        public int Height { get; set; }

        public BlockHeightModel()
        {
        }

        public BlockHeightModel(IBlockInfo blockInfo)
        {
            Hash = blockInfo.Hash;
            Height = blockInfo.Height;
        }
    }
}