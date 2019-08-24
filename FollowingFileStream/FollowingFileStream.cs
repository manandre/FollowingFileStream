using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FollowingFileStream
{
    /// <summary>
    /// Provides a System.IO.Stream for following a file being written,
    /// supporting both synchronous and asynchronous read operations.
    /// </summary>
    public class FollowingFileStream : Stream
    {
        /// <summary>
        /// The underlying filestream
        /// </summary>
        private readonly FileStream fileStream;
        /// <summary>
        /// Time before retrying write access to followed file
        /// </summary>
        private const int MillisecondsRetryTimeout = 100;
        /// <summary>
        /// Cancellation token source for retry attempts
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        /// <summary>
        /// Asynchronous lock to avoid race conditions
        /// </summary>
        private readonly AsyncLock locker = new AsyncLock();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the FollowingFileStream class with the specified path.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file
        /// that the current FollowingFileStream object will encapsulate.</param>
        /// <exception cref="System.ArgumentException">
        /// path is an empty string (&quot;&quot;), contains only white space, or contains
        /// one or more invalid characters. -or- path refers to a non-file device, such as
        /// &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;,
        /// &quot;lpt1:&quot;, etc. in a non-NTFS environment.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// The file cannot be found. The file must already exist.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The stream has been closed.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        public FollowingFileStream(string path)
        {
            fileStream = new FileStream(path, FileMode.Open,FileAccess.Read, FileShare.ReadWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">A relative or absolute path for the file
        /// that the current FollowingFileStream object will encapsulate.</param>
        /// <param name="bufferSize">A positive System.Int32 value greater than 0 indicating the buffer size. The
        /// default buffer size is 4096.</param>
        /// <param name="useAsync">Specifies whether to use asynchronous I/O or synchronous I/O. However, note that
        /// the underlying operating system might not support asynchronous I/O, so when specifying
        /// true, the handle might be opened synchronously depending on the platform. When
        /// opened asynchronously, the System.IO.FileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)
        /// and System.IO.FileStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)
        /// methods perform better on large reads or writes, but they might be much slower
        /// for small reads or writes. If the application is designed to take advantage of
        /// asynchronous I/O, set the useAsync parameter to true. Using asynchronous I/O
        /// correctly can speed up applications by as much as a factor of 10, but using it
        /// without redesigning the application for asynchronous I/O can decrease performance
        /// by as much as a factor of 10.</param>
        /// <exception cref="System.ArgumentException">
        /// path is an empty string (&quot;&quot;), contains only white space, or contains
        /// one or more invalid characters. -or- path refers to a non-file device, such as
        /// &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// path refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;,
        /// &quot;lpt1:&quot;, etc. in a non-NTFS environment.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// path is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// bufferSize is negative or zero.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// The file cannot be found. The file must already exist.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The stream has been closed.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        public FollowingFileStream(string path, int bufferSize, bool useAsync)
        {
             fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, useAsync);
        }

        /// <summary>
        /// Gets the name of the FollowingFileStream that was passed to the constructor.
        /// </summary>
        /// <value>A string that is the name of the FollowingFileStream.</value>
        public virtual string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the FollowingFileStream was opened asynchronously or synchronously.
        /// </summary>
        /// <returns>
        /// true if the FollowongFileStream was opened asynchronously; otherwise, false.
        /// </returns>
        public virtual bool IsAsync => fileStream.IsAsync;
        #endregion

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// Always false.
        /// </returns>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; false if the stream is closed.
        /// </returns>
        public override bool CanRead => fileStream.CanRead;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; false if the stream is closed.
        /// </returns>
        public override bool CanSeek => fileStream.CanSeek;

        public override bool CanTimeout => fileStream.CanTimeout;

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanSeek for this stream is false.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error, such as the file being closed, occurred.
        /// </exception>
        public override long Length => fileStream.Length;

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        /// <returns>The current position of this stream.</returns>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanSeek for this stream is false.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error, such as the file being closed, occurred.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Attempted to set the position to a negative value.
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">
        /// Attempted seeking past the end of a stream that does not support this.
        /// </exception>
        public override long Position { get => fileStream.Position; set => fileStream.Position = value;}

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between
        /// offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This might be less than the number
        /// of bytes requested if that number of bytes are not currently available, or zero
        /// if the end of the stream is reached.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanRead for this stream is false.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset,count, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the
        /// position within the stream by the number of bytes read, and monitors cancellation
        /// requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the total number of bytes read into the buffer. The result
        /// value can be less than the number of bytes requested if the number of bytes currently
        /// available is less than the requested number, or it can be 0 (zero) if the end
        /// of the stream has been reached.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanRead for this stream is false.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The stream is currently in use by a previous read operation.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = 0;
            using(await locker.LockAsync())
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                do {
                    try{
                        read = await fileStream.ReadAsync(buffer, offset, count, linkedCts.Token);
                    }
                    catch (OperationCanceledException) {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                } while (read == 0 && await RetryNeededAsync());
            }
            return read;
        }

        /// <summary>
        /// Asynchronously checks wheter the file is locked for writing
        /// and waits for a while according to MillisecondsRetryTimeout.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous retry operation. The value of the TResult
        /// parameter contains the boolean result. The result
        /// value can be false if the stream is closed.
        /// </returns>
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

        /// <summary>
        /// Synchronously checks whether the file is locked for writing
        /// </summary>
        /// <returns> true if the file is locked for writing; false, otherwise.</returns>
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

        /// <summary>
        /// Releases the unmanaged resources used by the FollowingFileStream and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.
        ///</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return; 
            
            if (disposing) {
                cts.Cancel();
                using(locker.Lock())
                {
                    fileStream.Dispose();
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

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Not supported</exception>
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

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Not supported</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        
        /// <summary>
        /// Writes a block of bytes to the file stream.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Not supported</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Begins an asynchronous read operation. (Consider using FollowingFileStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)
        /// instead.)
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="offset">The byte offset in array at which to begin reading.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">The method to be called when the asynchronous read operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request
        /// from other requests.</param>
        /// <returns>An object that references the asynchronous read.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanRead for this stream is false.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The stream is currently in use by a previous read operation.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An asynchronous read was attempted past the end of the file.
        /// </exception>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).AsApm(callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous read operation to complete. (Consider using
        /// FollowingFileStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)
        /// instead.)
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to wait for.</param>
        /// <returns>
        /// The number of bytes read from the stream, between 0 and the number of bytes you
        /// requested. Streams only return 0 at the end of the stream, otherwise, they should
        /// block until at least 1 byte is available.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// asyncResult is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// This System.IAsyncResult object was not created by calling FollwingFileStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)
        /// on this class.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// FollowingFileStream.EndRead(System.IAsyncResult) is called multiple times.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The stream is closed or an internal error has occurred.
        /// </exception>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }
    }

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