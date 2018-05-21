using System.Threading.Tasks;
using NBitcoin;

namespace Zorbit.Features.Indexer.Model.Checkpoints
{
    public interface ICheckpoint
    {
        BlockLocator BlockLocator { get; }
        CheckpointType CheckpointType { get; }
        uint256 Genesis { get; }

        Task DeleteAsync();
        bool SaveProgress(BlockLocator locator);
        bool SaveProgress(ChainedBlock tip);
        string ToString();
    }
}