using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manandre.Threading
{
    public sealed class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<AsyncLock> LockAsync()
        {
            await _semaphoreSlim.WaitAsync();
            return this;
        }

        public AsyncLock Lock()
        {
            return LockAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }
}