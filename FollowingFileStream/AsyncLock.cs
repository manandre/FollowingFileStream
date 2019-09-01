using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manandre.Threading
{
    /// <summary>
    /// This is the async-ready almost-equivalent of the lock keyword or the Mutex type, similar to Stephen Toub's AsyncLock.
    /// </summary>
    /// <remarks>
    /// It's only almost equivalent because the lock keyword permits reentrancy,
    /// which is not currently possible to do with an async-ready lock.
    /// An AsyncLock is either taken or not. The lock can be asynchronously acquired by calling LockAsync,
    /// and it is released by disposing the result of that task.
    /// 
    /// </remarks>
    public sealed class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Asynchronous method to request lock.
        /// </summary>
        /// <param name="cancellationToken">AsyncLock takes an optional CancellationToken, which can be used to cancel the acquiring of the lock.</param>
        /// <returns>
        /// The task returned from LockAsync will enter the Completed state when it has acquired the AsyncLock.
        /// That same task will enter the Canceled state if the CancellationToken is signaled before the wait is satisfied;
        /// in that case, the AsyncLock is not taken by that task.
        /// </returns>
        public async Task<AsyncLock> LockAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            return this;
        }

        /// <summary>
        /// Synchronous method to request lock.
        /// </summary>
        /// <param name="cancellationToken">AsyncLock takes an optional CancellationToken, which can be used to cancel the acquiring of the lock.</param>
        /// <returns>An instance of AsyncLock</returns>
        public AsyncLock Lock(CancellationToken cancellationToken = default(CancellationToken))
        {
            return LockAsync(cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Dispose method to release the lock.
        /// </summary>
        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }
}