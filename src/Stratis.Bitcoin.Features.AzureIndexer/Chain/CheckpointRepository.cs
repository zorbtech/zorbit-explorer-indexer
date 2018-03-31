using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public class CheckpointRepository
    {
        private readonly CloudBlobContainer _container;
        private readonly Network _network;

        public CheckpointRepository(CloudBlobContainer container, Network network, string checkpointSet)
        {
            _network = network;
            _container = container;
            CheckpointSet = checkpointSet;
        }

        public Task<Checkpoint> GetCheckpointAsync(string checkpointName)
        {
            var blob = _container.GetBlockBlobReference($"Checkpoints/{GetSetPart(checkpointName)}");
            return Checkpoint.LoadBlobAsync(blob, _network);
        }


        public async Task<Checkpoint[]> GetCheckpointsAsync()
        {
            var checkpoints = new List<Task<Checkpoint>>();
            var blobs = await _container.ListBlobsAsync($"Checkpoints/{GetSetPart()}", true, BlobListingDetails.None);
            foreach (var blob in blobs.OfType<CloudBlockBlob>())
            {
                checkpoints.Add(Checkpoint.LoadBlobAsync(blob, _network));
            }

            return await Task.WhenAll(checkpoints.ToArray());
        }

        public Checkpoint GetCheckpoint(string checkpointName)
        {
            try
            {
                return GetCheckpointAsync(checkpointName).Result;
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                return null;
            }
        }

        public async Task DeleteCheckpointsAsync()
        {
            var checkpoints = await GetCheckpointsAsync().ConfigureAwait(false);
            await Task.WhenAll(checkpoints.Select(checkpoint => checkpoint.DeleteAsync()).ToArray()).ConfigureAwait(false);
        }

        public void DeleteCheckpoints()
        {
            try
            {
                DeleteCheckpointsAsync().Wait();
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
            }
        }

        private string GetSetPart(string checkpointName)
        {
            var isLocal = !checkpointName.Contains('/');
            if (isLocal)
                return GetSetPart() + checkpointName;
            if (checkpointName.StartsWith("/"))
                checkpointName = checkpointName.Substring(1);
            return checkpointName;
        }

        private string GetSetPart()
        {
            if (CheckpointSet == null)
                return "";
            return CheckpointSet + "/";
        }

        public string CheckpointSet { get; set; }
    }

    public static class CloudBlobContainerExtensions
    {
        public static async Task<IList<IListBlobItem>> ListBlobsAsync(this CloudBlobContainer blobContainer, string prefix, 
            bool useFlatBlobListing, BlobListingDetails blobListingDetails, CancellationToken ct = default(CancellationToken), 
            Action<IList<IListBlobItem>> onProgress = null)
        {
            var items = new List<IListBlobItem>();
            BlobContinuationToken token = null;

            do
            {
                var seg = await blobContainer.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, null, token, null, null, ct);
                token = seg.ContinuationToken;
                items.AddRange(seg.Results);
                onProgress?.Invoke(items);

            } while (token != null && !ct.IsCancellationRequested);

            return items;
        }
    }
}
