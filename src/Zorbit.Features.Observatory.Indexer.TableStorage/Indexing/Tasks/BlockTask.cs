using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Adapters;

namespace Zorbit.Features.Observatory.TableStorage.Indexing.Tasks
{
    public sealed class BlockTask : IndexerTableTask
    {
        public BlockTask(
            AzureStorageClient storageClient,
            IndexerSettings settings)
            : base(storageClient, settings)
        {
        }

        protected override CloudTable GetCloudTable()
        {
            return StorageClient.GetBlockTable();
        }

        protected override Task<IEnumerable<ITaskAdapter>> GetTasksAsync(IEnumerable<IBlockInfo> blocks)
        {
            var info = blocks.Select(b => new BlockInfoAdapter(new BlockInfoModel(b)));
            var chunks = info.SelectMany(b => b.GetChunks())
                .OfType<BlockChunkModel>()
                .Select(c => new BlockChunkAdapter(c));
            var heights = blocks.Select(b => new BlockHeightAdapter(new BlockHeightModel(b)));

            var result = new List<ITaskAdapter>();
            result.AddRange(info);
            result.AddRange(chunks);
            result.AddRange(heights);
            return Task.FromResult(result.AsEnumerable());
        }
    }
}