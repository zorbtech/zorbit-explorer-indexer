using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NBitcoin;
using Stratis.Bitcoin;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Indexing;
using Zorbit.Features.Observatory.Core.Model;
using Zorbit.Features.Observatory.TableStorage.Adapters;

namespace Zorbit.Features.Observatory.TableStorage.Indexing
{
    public class CheckpointStore : ICheckpointStore
    {
        private readonly AzureStorageClient _storageClient;
        private readonly Network _network;
        private readonly CancellationToken _cancellationToken;
        private readonly bool _saveProgress;

        public CheckpointStore(
            IFullNode node,
            AzureStorageClient storageClient,
            IndexerSettings settings)
        {
            _network = node.Network;
            _storageClient = storageClient;
            _saveProgress = !settings.IgnoreCheckpoints;
            _cancellationToken = node.NodeLifetime.ApplicationStopping;
        }

        public async Task<ICheckpoint> GetCheckpointAsync(IndexType indexType)
        {
            if (_saveProgress)
            {
                var paritionKey = indexType.ToString().ToLowerInvariant();
                var operation = TableOperation.Retrieve<CheckpointAdapter>(paritionKey, string.Empty);
                var response = await _storageClient.GetCheckpointTable().ExecuteAsync(
                    operation, new TableRequestOptions(), new OperationContext(), _cancellationToken);
                if (response.Result != null)
                {
                    return (CheckpointAdapter)response.Result;
                }
            }

            var checkpoint = new CheckpointModel
            {
                IndexType = indexType
            };
            var genesisHash = _network.GetGenesis().Header.GetHash();
            checkpoint.BlockLocator.Blocks.Add(genesisHash);
            return new CheckpointAdapter(checkpoint);
        }

        public async Task<bool> SaveCheckpointAsync(ICheckpoint checkpoint)
        {
            if (!_saveProgress)
            {
                return false;
            }

            var operation = TableOperation.InsertOrReplace((CheckpointAdapter)checkpoint);
            var result = await _storageClient.GetCheckpointTable().ExecuteAsync(operation,
                new TableRequestOptions(),
                new OperationContext(),
                _cancellationToken);
            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteCheckpointAsync(ICheckpoint checkpoint)
        {
            var operation = TableOperation.Delete((CheckpointAdapter)checkpoint);
            var result = await _storageClient.GetCheckpointTable().ExecuteAsync(operation,
                new TableRequestOptions(),
                new OperationContext(),
                _cancellationToken);
            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }
    }
}
