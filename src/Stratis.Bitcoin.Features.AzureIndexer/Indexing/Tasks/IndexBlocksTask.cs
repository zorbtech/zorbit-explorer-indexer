using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NBitcoin;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public class IndexBlocksTask : IndexTask<BlockInfo>
    {
        private volatile int _indexedBlocks;

        public IndexBlocksTask(IndexerConfiguration configuration)
            : base(configuration)
        {
        }

        public void Index(Block[] blocks, TaskScheduler taskScheduler)
        {
            if (taskScheduler == null)
            {
                throw new ArgumentNullException("taskScheduler");
            }

            try
            {
                IndexAsync(blocks, taskScheduler).Wait();
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                throw;
            }
        }

        public Task IndexAsync(Block[] blocks, TaskScheduler taskScheduler)
        {
            if (taskScheduler == null)
            {
                throw new ArgumentNullException("taskScheduler");
            }

            var tasks = blocks
                .Select(b => new Task(() => IndexCore("o", new[]{new BlockInfo()
                {
                    Block = b,
                    BlockId = b.GetHash()
                }})))
                .ToArray();

            foreach (var t in tasks)
            {
                t.Start(taskScheduler);
            }

            return Task.WhenAll(tasks);
        }

        protected override async Task EnsureSetup()
        {
            await Configuration.GetBlocksContainer().CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        protected override void ProcessBlock(BlockInfo block, BulkImport<BlockInfo> bulk)
        {
            bulk.Add("o", block);
        }

        protected override void IndexCore(string partitionName, IEnumerable<BlockInfo> blocks)
        {
            var first = blocks.First();
            var block = first.Block;
            var hash = first.BlockId.ToString();

            var watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                var container = Configuration.GetBlocksContainer();
                var client = container.ServiceClient;
                client.DefaultRequestOptions.SingleBlobUploadThresholdInBytes = 32 * 1024 * 1024;
                var blob = container.GetPageBlobReference(hash);
                var ms = new MemoryStream();
                block.ReadWrite(ms, true);
                var blockBytes = ms.GetBuffer();

                var length = 512 - (ms.Length % 512);
                if (length == 512)
                {
                    length = 0;
                }

                Array.Resize(ref blockBytes, (int)(ms.Length + length));

                try
                {
                    blob.
                        UploadFromByteArrayAsync(blockBytes, 0, blockBytes.Length, new AccessCondition()
                        {
                            //Will throw if already exist, save 1 call
                            IfNotModifiedSinceTime = DateTimeOffset.MinValue
                        }, new BlobRequestOptions()
                        {
                            MaximumExecutionTime = Timeout,
                            ServerTimeout = Timeout
                        }
                    , new OperationContext()).GetAwaiter().GetResult();
                    watch.Stop();
                    IndexerTrace.BlockUploaded(watch.Elapsed, blockBytes.Length);
                    _indexedBlocks++;
                    break;
                }
                catch (StorageException ex)
                {
                    var alreadyExist = ex.RequestInformation != null && ex.RequestInformation.HttpStatusCode == 412;
                    if (!alreadyExist)
                    {
                        IndexerTrace.ErrorWhileImportingBlockToAzure(uint256.Parse(hash), ex);
                        throw;
                    }
                    watch.Stop();
                    IndexerTrace.BlockAlreadyUploaded();
                    _indexedBlocks++;
                    break;
                }
                catch (Exception ex)
                {
                    IndexerTrace.ErrorWhileImportingBlockToAzure(uint256.Parse(hash), ex);
                    throw;
                }
            }
        }

        protected override int PartitionSize => 1;

        public int IndexedBlocks => _indexedBlocks;
    }
}
