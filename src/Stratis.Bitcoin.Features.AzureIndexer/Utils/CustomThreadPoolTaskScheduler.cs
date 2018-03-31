using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    public class CustomThreadPoolTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly AutoResetEvent _finished = new AutoResetEvent(false);
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly BlockingCollection<Task> _tasks;
        private int _availableThreads;
        private bool _disposed;

        public CustomThreadPoolTaskScheduler(int threadCount, int maxQueued, string name = null)
        {
            ThreadsCount = threadCount;
            _tasks = new BlockingCollection<Task>(new ConcurrentQueue<Task>(), maxQueued);
            _availableThreads = threadCount;
            for (var i = 0 ; i < threadCount ; i++)
            {
                new Thread(Do)
                {
                    IsBackground = true,
                    Name = name
                }.Start();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _cancel.Cancel();
        }

        public void WaitFinished()
        {
            AssertNotDisposed();
            while (true)
            {
                if (_disposed)
                    return;
                if (RemainingTasks == 0)
                    return;
                _finished.WaitOne(1000);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        protected override void QueueTask(Task task)
        {
            AssertNotDisposed();
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            AssertNotDisposed();
            return false;
        }

        private void Do(object state)
        {
            try
            {
                foreach (var task in _tasks.GetConsumingEnumerable(_cancel.Token))
                {
                    Interlocked.Decrement(ref _availableThreads);
                    TryExecuteTask(task);
                    Interlocked.Increment(ref _availableThreads);
                    if (RemainingTasks == 0)
                        _finished.Set();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("CustomThreadPoolTaskScheduler");
        }

        public override int MaximumConcurrencyLevel => ThreadsCount;

        public int QueuedCount => _tasks.Count;

        public int AvailableThreads => _availableThreads;

        public int RemainingTasks => (ThreadsCount - AvailableThreads) + QueuedCount;

        public int ThreadsCount { get; }
    }
}
