using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FollowingFileStream
{
    public class FollowingFileStream : Stream
    {
        private FileStream fileStream;
        private const int MillisecondsRetryTimeout = 100;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private readonly AsyncLock locker = new AsyncLock();

        #region Constructors
        public FollowingFileStream(string path, FileMode mode)
        {
            fileStream = new FileStream(path, mode);
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access)
        {
            fileStream = new FileStream(path, mode, access);
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            fileStream = new FileStream(path, mode, access, share);
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            fileStream = new FileStream(path, mode, access, share, bufferSize);
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
             fileStream = new FileStream(path, mode, access, share, bufferSize, useAsync);
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
             fileStream = new FileStream(path, mode, access, share, bufferSize, options);
        }
        #endregion
        public override bool CanWrite => false;

        public override bool CanRead => fileStream.CanRead;

        public override bool CanSeek => fileStream.CanSeek;

        public override bool CanTimeout => fileStream.CanTimeout;

        public override long Length => fileStream.Length;

        public override long Position { get => fileStream.Position; set => fileStream.Position = value;}

        public override int Read(byte[] array, int offset, int count)
        {
            return ReadAsync(array, offset,count, CancellationToken.None).GetAwaiter().GetResult();
        }

        public override async Task<int> ReadAsync(byte[] array, int offset, int count, CancellationToken cancellationToken)
        {
            int read = 0;
            using(await locker.LockAsync())
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                do {
                    try{
                        read = await fileStream.ReadAsync(array, offset, count, linkedCts.Token);
                    }
                    catch (OperationCanceledException) {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                } while (read == 0 && await RetryNeededAsync());
            }
            return read;
        }

        private async Task<bool> RetryNeededAsync()
        {
            bool retry = IsFileLockedForWriting();
            if (retry) {
                try
                {
                    await Task.Delay(MillisecondsRetryTimeout, cts.Token).ConfigureAwait(false);
                }
                catch(TaskCanceledException)
                {
                    retry = false;
                }
            }
            return retry;
        }

        private bool IsFileLockedForWriting()
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(fileStream.Name, FileMode.Open, FileAccess.Write, FileShare.Read);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return; 
            
            if (disposing) {
                cts.Cancel();
                using(locker.Lock())
                {
                    fileStream?.Dispose();
                }
                cts.Dispose();
                // Free any other managed objects here.
                //
            }
            
            // Free any unmanaged objects here.
            //

            disposed = true;
            // Call fileStream class implementation.
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            using(locker.Lock())
            {
                return fileStream.Seek(offset, origin);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).AsApm(callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }
    }

    public class AsyncLock : IDisposable
    {
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
 
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

    public static class AsyncExtensions
    {
        public static IAsyncResult AsApm<T>(this Task<T> task, 
                                            AsyncCallback callback, 
                                            object state)
        {
            if (task == null) 
                throw new ArgumentNullException("task");
            
            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(t => 
                            {
                                if (t.IsFaulted) 
                                    tcs.TrySetException(t.Exception.InnerExceptions);
                                else if (t.IsCanceled)    
                                    tcs.TrySetCanceled();
                                else 
                                    tcs.TrySetResult(t.Result);

                                if (callback != null) 
                                    callback(tcs.Task);
                            }, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}