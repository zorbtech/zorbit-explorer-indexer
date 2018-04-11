using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks
{
    public sealed class BlockSummaryTaskFactory : IIndexTaskFactory<BlockSummaryTask>
    {
        private readonly string _partitionKey;
        private readonly AzureStorageClient _storageClient;
        private readonly AzureIndexerSettings _settings;

        public BlockSummaryTaskFactory(
            FullNode fullNode,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings)
        {
            _partitionKey = fullNode.Network.Name.ToLowerInvariant();
            _storageClient = storageClient;
            _settings = settings;
        }

        public BlockSummaryTask CreateTask()
        {
            return new BlockSummaryTask(_partitionKey, _storageClient)
            {
                SaveProgression = !_settings.IgnoreCheckpoints
            };
        }
    }
}
