using System.Threading.Tasks;
using Zorbit.Features.Observatory.Core.Model;

namespace Zorbit.Features.Observatory.Core.Indexing
{
    public interface ICheckpointStore
    {
        Task<ICheckpoint> GetCheckpointAsync(IndexType index);
        Task<bool> SaveCheckpointAsync(ICheckpoint checkpoint);
        Task<bool> DeleteCheckpointAsync(ICheckpoint checkpoint);
    }
}