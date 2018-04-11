using System;
using System.Collections.Generic;
using System.Text;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks
{
    public sealed class BlockTaskFactory : IIndexTaskFactory<BlockTask>
    {
        private readonly string _partitionKey;
        private readonly AzureStorageClient _storageClient;
        private readonly AzureIndexerSettings _settings;

        public BlockTaskFactory(
            FullNode fullNode,
            AzureStorageClient storageClient,
            AzureIndexerSettings settings)
        {
            _partitionKey = fullNode.Network.Name.ToLowerInvariant();
            _storageClient = storageClient;
            _settings = settings;
        }

        public BlockTask CreateTask()
        {
            return new BlockTask(_partitionKey, _storageClient)
            {
                SaveProgression = !_settings.IgnoreCheckpoints
            };
        }
    }
}
