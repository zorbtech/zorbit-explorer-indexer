using System.Linq;
using NBitcoin;

namespace Zorbit.Features.Observatory.Core.Model
{
    public interface ICheckpoint
    {
        IndexType IndexType { get; set; }
        BlockLocator BlockLocator { get; set; }
        uint256 Genesis { get; }
    }

    public sealed class CheckpointModel : ICheckpoint
    {
        public IndexType IndexType { get; set; }
        public BlockLocator BlockLocator { get; set; } = new BlockLocator();
        public uint256 Genesis => BlockLocator?.Blocks.First();
    }
}