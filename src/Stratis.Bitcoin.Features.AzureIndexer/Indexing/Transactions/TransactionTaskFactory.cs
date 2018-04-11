using System;
using System.Collections.Generic;
using System.Text;
using Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Blocks
{
    public sealed class TransactionTaskFactory : IIndexTaskFactory<TransactionTask>
    {
        private readonly AzureStorageClient _storageClient;
        private readonly AzureIndexerSettings _settings;

        public TransactionTaskFactory(
            AzureStorageClient storageClient,
            AzureIndexerSettings settings)
        {
            _storageClient = storageClient;
            _settings = settings;
        }

        public TransactionTask CreateTask()
        {
            return new TransactionTask(_storageClient)
            {
                SaveProgression = !_settings.IgnoreCheckpoints
            };
        }
    }
}
