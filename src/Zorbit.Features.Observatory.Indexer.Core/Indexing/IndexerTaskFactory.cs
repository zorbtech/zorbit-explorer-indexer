using Zorbit.Features.Observatory.Core.Model;

namespace Zorbit.Features.Observatory.Core.Indexing
{
    public interface IIndexerTaskFactory
    {
        IIndexerTask CreateTask(IndexType indexType);
    }
}