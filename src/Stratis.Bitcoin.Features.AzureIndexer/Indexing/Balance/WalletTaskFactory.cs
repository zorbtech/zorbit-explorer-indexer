using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks
{
    public sealed class WalletTaskFactory : IIndexTaskFactory<BalanceTask>
    {
        private readonly string _partitionKey;
        private readonly AzureStorageClient _storageClient;
        private readonly AzureIndexerSettings _settings;

        public WalletTaskFactory(
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
            // todo this.IndexerConfig.CreateIndexerClient().GetAllWalletRules()
            return new BalanceTask(_partitionKey, _storageClient, null)
            {
                SaveProgression = !_settings.IgnoreCheckpoints
            };
        }
    }
}
