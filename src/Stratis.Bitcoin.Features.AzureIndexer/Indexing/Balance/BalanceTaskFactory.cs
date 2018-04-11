using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks
{
    public sealed class BalanceTaskFactory : IIndexTaskFactory<BalanceTask>
    {
        private readonly string _partitionKey;
        private readonly AzureStorageClient _storageClient;
        private readonly AzureIndexerSettings _settings;

        public BalanceTaskFactory(
            FullNode fullNode,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings)
        {
            _partitionKey = fullNode.Network.Name.ToLowerInvariant();
            _storageClient = storageClient;
            _settings = settings;
        }

        public BalanceTask CreateTask()
        {
            return new BalanceTask(_partitionKey, _storageClient, null)
            {
                SaveProgression = !_settings.IgnoreCheckpoints
            };
        }
    }
}
