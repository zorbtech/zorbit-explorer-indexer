using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
    /// <summary>
    /// A retry strategy with back-off parameters for calculating the exponential delay between retries.
    /// </summary>
    internal class ExponentialBackoff
    {
        private readonly int _retryCount;
        private readonly TimeSpan _minBackoff;
        private readonly TimeSpan _maxBackoff;
        private readonly TimeSpan _deltaBackoff;


        public ExponentialBackoff(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        {
            this._retryCount = retryCount;
            this._minBackoff = minBackoff;
            this._maxBackoff = maxBackoff;
            this._deltaBackoff = deltaBackoff;
        }

        public async Task Do(Action act, TaskScheduler scheduler = null)
        {
            Exception lastException = null;
            var retryCount = -1;

            TimeSpan wait;

            while (true)
            {
                try
                {
                    var task = new Task(act);
                    task.Start(scheduler);
                    await task.ConfigureAwait(false);
                    break;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                retryCount++;
                if (!GetShouldRetry(retryCount, lastException, out wait))
                {
                    ExceptionDispatchInfo.Capture(lastException).Throw();
                }
                else
                {
                    await Task.Delay(wait).ConfigureAwait(false);
                }
            }
        }

        internal bool GetShouldRetry(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
        {
            if (currentRetryCount < this._retryCount)
            {
                var random = new Random();

                var delta = (int)((Math.Pow(2.0, currentRetryCount) - 1.0) * random.Next((int)(this._deltaBackoff.TotalMilliseconds * 0.8), (int)(this._deltaBackoff.TotalMilliseconds * 1.2)));
                var interval = (int)Math.Min(checked(this._minBackoff.TotalMilliseconds + delta), this._maxBackoff.TotalMilliseconds);
                retryInterval = TimeSpan.FromMilliseconds(interval);

                return true;
            }

            retryInterval = TimeSpan.Zero;
            return false;
        }
    }
}