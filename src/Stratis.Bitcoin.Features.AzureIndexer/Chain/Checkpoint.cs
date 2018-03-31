using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Chain
{
    public class Checkpoint
    {
        private readonly CloudBlockBlob _blob;

        public Checkpoint(string checkpointName, Network network, Stream data, CloudBlockBlob blob)
        {
            _blob = blob;
            CheckpointName = checkpointName ?? throw new ArgumentNullException("checkpointName");
            BlockLocator = new BlockLocator();

            if (data != null)
            {
                try
                {
                    BlockLocator.ReadWrite(data, false);
                    return;
                }
                catch
                {
                }
            }

            var list = new List<uint256>
            {
                network.GetGenesis().Header.GetHash() 
            };

            BlockLocator = new BlockLocator();
            BlockLocator.Blocks.AddRange(list);
        }

        public static async Task<Checkpoint> LoadBlobAsync(CloudBlockBlob blob, Network network)
        {
            var checkpointName = string.Join("/", blob.Name.Split('/').Skip(1).ToArray());
            var ms = new MemoryStream();
            try
            {
                await blob.DownloadToStreamAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation == null || ex.RequestInformation.HttpStatusCode != 404)
                    throw;
            }
            var checkpoint = new Checkpoint(checkpointName, network, ms, blob);
            return checkpoint;
        }

        public bool SaveProgress(ChainedBlock tip)
        {
            return SaveProgress(tip.GetLocator());
        }

        public bool SaveProgress(BlockLocator locator)
        {
            BlockLocator = locator;
            try
            {
                return SaveProgressAsync().Result;
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                return false;
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                await _blob.DeleteAsync().ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation != null && ex.RequestInformation.HttpStatusCode == 404)
                    return;
                throw;
            }
        }

        public override string ToString()
        {
            return CheckpointName;
        }

        private async Task<bool> SaveProgressAsync()
        {
            var bytes = BlockLocator.ToBytes();
            try
            {

                await _blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length, new AccessCondition()
                {
                    IfMatchETag = _blob.Properties.ETag
                }, null, null).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation != null && ex.RequestInformation.HttpStatusCode == 412)
                    return false;
                throw;
            }
            return true;
        }

        public BlockLocator BlockLocator { get; private set; }

        public string CheckpointName { get; }

        public uint256 Genesis => BlockLocator.Blocks[BlockLocator.Blocks.Count - 1];
    }
}
