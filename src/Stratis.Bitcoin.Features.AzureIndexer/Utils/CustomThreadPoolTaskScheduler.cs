using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
    public class CustomThreadPoolTaskScheduler : TaskScheduler, IDisposable
    {
        private readonly int _threadCount;
        public CustomThreadPoolTaskScheduler(int threadCount, int maxQueued, string name = null)
        {
            _threadCount = threadCount;
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

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return _threadCount;
            }
        }

        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

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

        public int QueuedCount
        {
            get
            {
                return _tasks.Count;
            }
        }

        private int _availableThreads;
        public int AvailableThreads
        {
            get
            {
                return _availableThreads;
            }
        }

        public int RemainingTasks
        {
            get
            {
                return (_threadCount - AvailableThreads) + QueuedCount;
            }
        }

        public int ThreadsCount
        {
            get
            {
                return _threadCount;
            }
        }

        private readonly BlockingCollection<Task> _tasks;
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

        #region IDisposable Members

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
            _cancel.Cancel();
        }

        #endregion

        private readonly AutoResetEvent _finished = new AutoResetEvent(false);
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

        private void AssertNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("CustomThreadPoolTaskScheduler");
        }
    }
}
