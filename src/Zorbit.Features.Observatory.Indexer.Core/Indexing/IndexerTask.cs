using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zorbit.Features.Observatory.Core.Model;

namespace Zorbit.Features.Observatory.Core.Indexing
{
    public interface IIndexerTask
    {
        Task RollBackAsync(IEnumerable<IBlockInfo> blocks);

        Task IndexAsync(IEnumerable<IBlockInfo> blocks);
    }

    public abstract class IndexerTask<TIndexed> : IIndexerTask
        where TIndexed : IBatchItem
    {
        protected IndexerTask(
            IndexerSettings settings)
        {
            Settings = settings;
        }

        public async Task IndexAsync(IEnumerable<IBlockInfo> blocks)
        {
            var tasks = await GetTasksAsync(blocks);
            await ProcessTasksAsync(tasks);
        }

        public Task RollBackAsync(IEnumerable<IBlockInfo> blocks)
        {
            return Task.CompletedTask;
        }

        protected abstract Task<IEnumerable<TIndexed>> GetTasksAsync(IEnumerable<IBlockInfo> blocks);

        protected abstract Task ProcessTasksAsync(IEnumerable<TIndexed> tasks);

        protected TimeSpan Timeout { get; } = TimeSpan.FromMinutes(5.0);

        protected IndexerSettings Settings { get; private set; }
    }

    public interface IBatchItem
    {
    }
}
