using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Polly;
using Polly.Retry;
using Zorbit.Features.Observatory.Core;
using Zorbit.Features.Observatory.Core.Indexing;

namespace Zorbit.Features.Observatory.TableStorage.Indexing
{
    public interface ITaskAdapter : ITableEntity, IBatchItem
    {
        new string PartitionKey { get; set; }

        new string RowKey { get; set; }

        int GetSize();
    }

    public abstract class IndexerTableTask : IndexerTask<ITaskAdapter>
    {
        private const int MaximumBatchOperations = 100;
        private const int MaximumBatchSize = 4000000;
        private const int MaximumEntitySize = 1000000;

        private readonly RetryPolicy _retry;

        protected IndexerTableTask(
            AzureStorageClient storageClient,
            IndexerSettings settings)
            : base(settings)
        {
            StorageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
            _retry = Policy.Handle<StorageException>()
                .WaitAndRetryAsync(15, backoff => TimeSpan.FromSeconds(Math.Pow(2, backoff)));
        }

        protected abstract CloudTable GetCloudTable();

        protected override async Task ProcessTasksAsync(IEnumerable<ITaskAdapter> tasks)
        {
            var table = GetCloudTable();

            var options = new TableRequestOptions()
            {
                PayloadFormat = TablePayloadFormat.Json,
                MaximumExecutionTime = Timeout,
                ServerTimeout = Timeout,
            };
            var context = new OperationContext();

            var taskCount = 0;
            var batchTasks = new List<Task<IList<TableResult>>>();
            var singleTasks = new List<Task<TableResult>>();

            var batches = GetBatches(tasks);
            foreach (var batch in batches)
            {
                taskCount++;
                if (batch.Count > 1)
                {
                    var t = _retry.ExecuteAsync(async () => await table.ExecuteBatchAsync(batch, options, context));
                    batchTasks.Add(t);
                }
                else if (batch.Count == 1)
                {
                    var t = _retry.ExecuteAsync(async () => await table.ExecuteAsync(batch.Single(), options, context));
                    singleTasks.Add(t);
                }

                if (taskCount < Settings.TaskCount)
                {
                    continue;
                }

                await Task.WhenAll(batchTasks);
                await Task.WhenAll(singleTasks);
                taskCount = 0;
            }

            await Task.WhenAll(batchTasks);
            await Task.WhenAll(singleTasks);
        }

        private static IEnumerable<TableBatchOperation> GetBatches(IEnumerable<ITaskAdapter> tasks)
        {
            var result = new List<TableBatchOperation>();

            var partitions = tasks.GroupBy(b => b.PartitionKey);

            foreach (var parititon in partitions)
            {
                var batch = new TableBatchOperation();
                result.Add(batch);

                var batchCount = 0;
                var batchSize = 0;

                foreach (var item in parititon)
                {
                    batchCount++;

                    var itemSize = item.GetSize();
                    if (itemSize > MaximumEntitySize)
                    {
                        throw new ArgumentOutOfRangeException(nameof(itemSize));
                    }

                    if (batchSize + itemSize <= MaximumBatchSize)
                    {
                        batchSize += itemSize;
                        batch.Add(TableOperation.InsertOrReplace(item));
                    }
                    else
                    {
                        batchCount = 0;
                        batchSize = itemSize;
                        batch = new TableBatchOperation
                        {
                            TableOperation.InsertOrReplace(item)
                        };
                        result.Add(batch);
                    }

                    if (batchCount < MaximumBatchOperations)
                    {
                        continue;
                    }

                    batchCount = 0;
                    batchSize = 0;
                    batch = new TableBatchOperation();
                    result.Add(batch);
                }
            }

            return result;
        }

        public AzureStorageClient StorageClient { get; }
    }
}