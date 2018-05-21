using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Adapters;

namespace Zorbit.Features.Observatory.TableStorage.Indexing.Tasks
{
    public class BlockSummaryTask : IndexerTableTask
    {
        public BlockSummaryTask(
            AzureStorageClient storageClient,
            IndexerSettings settings)
            : base(storageClient, settings)
        {
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetBlockSummaryTable();
        }

        protected override Task<IEnumerable<ITaskAdapter>> GetTasksAsync(IEnumerable<IBlockInfo> blocks)
        {
            var result = new List<ITaskAdapter>();
            result.AddRange(blocks.Select(b => new BlockSummaryAdapter(new BlockSummaryModel(b))));
            return Task.FromResult(result.AsEnumerable());
        }
    }
}