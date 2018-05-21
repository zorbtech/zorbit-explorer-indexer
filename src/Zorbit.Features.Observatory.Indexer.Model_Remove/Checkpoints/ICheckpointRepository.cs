using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zorbit.Features.Indexer.Model.Checkpoints
{
    public interface ICheckpointRepository
    {
        void DeleteCheckpoints();
        Task DeleteCheckpointsAsync();
        ICheckpoint GetCheckpoint(string checkpointName);
        Task<ICheckpoint> GetCheckpointAsync(string checkpointName);
        Task<IEnumerable<ICheckpoint>> GetCheckpointsAsync();
    }
}