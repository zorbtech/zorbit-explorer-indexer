using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Stratis.Bitcoin.Features.AzureIndexer.Chain;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing.Tasks
{
    public interface IIndexTask
    {
        void Index(BlockFetcher blockFetcher, TaskScheduler scheduler);

        bool SaveProgression { get; set; }

        bool EnsureIsSetup { get; set; }
    }

    public abstract class IndexTask<TIndexed> : IIndexTask
    {

        private readonly ExponentialBackoff _retry = new ExponentialBackoff(15, TimeSpan.FromMilliseconds(100),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMilliseconds(200));
        private Exception _taskException;

        protected IndexTask(IndexerConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException("configuration");
            SaveProgression = true;
            MaxQueued = 100;
        }

        public void Index(BlockFetcher blockFetcher, TaskScheduler scheduler)
        {
            var tasks = new ConcurrentDictionary<Task, Task>();
            try
            {
                SetThrottling();
                if (EnsureIsSetup)
                    EnsureSetup().Wait();

                var bulk = new BulkImport<TIndexed>(PartitionSize);
                if (!SkipToEnd)
                {
                    try
                    {

                        foreach (var block in blockFetcher)
                        {
                            ThrowIfException();
                            if (blockFetcher.NeedSave)
                            {
                                if (SaveProgression)
                                {
                                    EnqueueTasks(tasks, bulk, true, scheduler);
                                    Save(tasks, blockFetcher, bulk);
                                }
                            }
                            ProcessBlock(block, bulk);
                            if (bulk.HasFullPartition)
                            {
                                EnqueueTasks(tasks, bulk, false, scheduler);
                            }
                        }
                        EnqueueTasks(tasks, bulk, true, scheduler);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken != blockFetcher.CancellationToken)
                            throw;
                    }
                }
                else
                    blockFetcher.SkipToEnd();
                if (SaveProgression)
                    Save(tasks, blockFetcher, bulk);
                WaitFinished(tasks);
                ThrowIfException();
            }
            catch (AggregateException aex)
            {
                ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                throw;
            }
        }

        protected abstract Task EnsureSetup();

        protected abstract void ProcessBlock(BlockInfo block, BulkImport<TIndexed> bulk);

        protected abstract void IndexCore(string partitionName, IEnumerable<TIndexed> items);

        private void EnqueueTasks(ConcurrentDictionary<Task, Task> tasks, BulkImport<TIndexed> bulk, bool uncompletePartitions, TaskScheduler scheduler)
        {
            if (!uncompletePartitions && !bulk.HasFullPartition)
            {
                return;
            }

            if (uncompletePartitions)
            {
                bulk.FlushUncompletePartitions();
            }

            while (bulk.ReadyPartitions.Count != 0)
            {
                var item = bulk.ReadyPartitions.Dequeue();

                var task = _retry.Do(() => IndexCore(item.Item1, item.Item2), scheduler);
                tasks.TryAdd(task, task);

                task.ContinueWith(prev =>
                {
                    _taskException = prev.Exception ?? _taskException;
                    tasks.TryRemove(prev, out prev);
                });

                if (tasks.Count > MaxQueued)
                {
                    WaitFinished(tasks, MaxQueued / 2);
                }
            }
        }

        private void SetThrottling()
        {
            Helper.SetThrottling();
            var tableServicePoint = ServicePointManager.FindServicePoint(Configuration.TableClient.BaseUri);
            tableServicePoint.ConnectionLimit = 1000;
        }

        private void Save(ICollection tasks, BlockFetcher fetcher, BulkImport<TIndexed> bulk)
        {
            WaitFinished(tasks);
            ThrowIfException();
            fetcher.SaveCheckpoint();
        }

        private static void WaitFinished(ICollection tasks, int queuedTarget = 0)
        {
            while (tasks.Count > queuedTarget)
            {
                Thread.Sleep(100);
            }
        }

        private void ThrowIfException()
        {
            if (_taskException != null)
            {
                ExceptionDispatchInfo.Capture(_taskException).Throw();
            }
        }

        public IndexerConfiguration Configuration { get; }

        public bool SaveProgression { get; set; }

        public int MaxQueued { get; set; }

        public bool EnsureIsSetup { get; set; } = true;

        protected abstract int PartitionSize { get; }

        /// <summary>
        /// Fast forward indexing to the end (if scanning not useful)
        /// </summary>
        protected virtual bool SkipToEnd => false;

        protected TimeSpan Timeout = TimeSpan.FromMinutes(5.0);
    }
}
