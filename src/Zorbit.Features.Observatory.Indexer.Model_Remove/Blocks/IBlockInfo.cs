using NBitcoin;

namespace Zorbit.Features.Indexer.Model.Blocks
{
    public interface IBlockInfo
    {
        Block Block { get; set; }
        uint256 BlockId { get; set; }
        int Height { get; set; }
    }
}