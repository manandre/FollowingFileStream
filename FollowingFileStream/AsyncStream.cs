using Manandre.Threading;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Manandre.IO
{
    /// <summary>
    /// Provides an asynchronous version of the System.IO.Stream class. 
    /// This is an abstract class.
    /// </summary>
    /// <remarks>
    /// It can be used to expose a System.IO.Stream class
    /// by implementing only the asynchronous operations: DoReadAsync, DoWriteAsync and DoFlushAsync 
    /// </remarks>
    public abstract class AsyncStream : Stream
    {
        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the
        /// position within the stream by the number of bytes read, and monitors cancellation
        /// requests. 
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="sync">If enabled, returns an already-completed task</param>
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
        protected abstract Task<int> DoReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, bool sync);

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
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => DoReadAsync(buffer, offset, count, cancellationToken, sync: false);

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
        /// AsyncStream.CanRead for this stream is false.
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
        => DoReadAsync(buffer, offset, count, CancellationToken.None, sync: true).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the
        /// current position within this stream by the number of bytes written, and monitors
        /// cancellation requests.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.
        /// The default value is System.Threading.CancellationToken.None.</param>
        /// <param name="sync">If enabled, returns an already-completed task</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// AsyncStream.CanWrite for this stream is false.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The stream is currently in use by a previous write operation.
        /// </exception>
        protected abstract Task DoWriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, bool sync);

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the
        /// current position within this stream by the number of bytes written, and monitors
        /// cancellation requests.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// AsyncStream.CanWrite for this stream is false.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The stream is currently in use by a previous write operation.
        /// </exception>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => DoWriteAsync(buffer, offset, count, cancellationToken, sync: false);

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream
        /// by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// AsyncStream.CanWrite for this stream is false.
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
        public override void Write(byte[] buffer, int offset, int count)
        => DoWriteAsync(buffer, offset, count, CancellationToken.None, sync: true).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to
        /// be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.
        /// The default value is System.Threading.CancellationToken.None.</param>
        /// <param name="sync">If enabled, returns an already-completed task</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        protected abstract Task DoFlushAsync(CancellationToken cancellationToken, bool sync);

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to
        /// be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.
        /// The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        => DoFlushAsync(cancellationToken, sync: false);

        /// <summary>
        /// Clears all buffers for this stream and causes
        /// any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// The stream is closed or an internal error has occurred.
        /// </exception>
        public override void Flush()
        => DoFlushAsync(CancellationToken.None, sync: true).GetAwaiter().GetResult();

#if !NETSTANDARD1_3

        /// <summary>
        /// Begins an asynchronous read operation. (Consider using AsyncStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)
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
        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        => ReadAsync(buffer, offset, count, CancellationToken.None).AsApm(callback, state);

        /// <summary>
        /// Begins an asynchronous write operation. (Consider using AsyncStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)
        /// instead.)
        /// </summary>
        /// <param name="buffer">The buffer to read data from.</param>
        /// <param name="offset">The byte offset in array at which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request
        /// from other requests.</param>
        /// <returns>An object that references the asynchronous write.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// buffer is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// offset and count describe an invalid range in array.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanWrite for this stream is false.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The stream is currently in use by a previous write operation.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// offset or count is negative.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An asynchronous write was attempted past the end of the file.
        /// </exception>
        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        => WriteAsync(buffer, offset, count, CancellationToken.None).AsApm(callback,state);

        /// <summary>
        /// Waits for the pending asynchronous read operation to complete. (Consider using
        /// AsyncStream.ReadAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)
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
        /// This System.IAsyncResult object was not created by calling AsyncStream.BeginRead(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)
        /// on this class.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// AsyncStream.EndRead(System.IAsyncResult) is called multiple times.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The stream is closed or an internal error has occurred.
        /// </exception>
        public sealed override int EndRead(IAsyncResult asyncResult)
        => ((Task<int>)asyncResult).GetAwaiter().GetResult();

        /// <summary>
        /// Waits for the pending asynchronous write operation to complete. (Consider using
        /// AsyncStream.WriteAsync(System.Byte[],System.Int32,System.Int32,System.Threading.CancellationToken)
        /// instead.)
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to wait for.</param>
        /// <exception cref="System.ArgumentNullException">
        /// asyncResult is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// This System.IAsyncResult object was not created by calling AsyncStream.BeginWrite(System.Byte[],System.Int32,System.Int32,System.AsyncCallback,System.Object)
        /// on this class.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// AsyncStream.EndWrite(System.IAsyncResult) is called multiple times.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// The stream is closed or an internal error has occurred.
        /// </exception>
        public sealed override void EndWrite(IAsyncResult asyncResult)
        => ((Task)asyncResult).GetAwaiter().GetResult();

#endif

        private bool disposed = false;

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

            disposed = true;
            // Call stream class implementation.
            base.Dispose(disposing);
        }

        /// <summary>
        /// Synchronized version of an async stream
        /// </summary>
        /// <param name="stream">Stream to synchronize</param>
        /// <returns></returns>
        public static AsyncStream Synchronized(AsyncStream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (stream is AsyncSafeStream)
            {
                return stream;
            }

            return new AsyncSafeStream(stream);
        }

        private sealed class AsyncSafeStream : AsyncStream
        {
            private readonly AsyncStream _stream;
            private readonly CancellationTokenSource cts = new CancellationTokenSource();
            private readonly AsyncLock locker = new AsyncLock();

            public AsyncSafeStream(AsyncStream stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }

                _stream = stream;
            }

            public override bool CanRead => _stream.CanRead;

            public override bool CanWrite => _stream.CanWrite;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanTimeout => _stream.CanTimeout;

            public override long Length
            {
                get
                {
                    using (locker.Lock(cts.Token))
                    {
                        return _stream.Length;
                    }
                }
            }

            public override long Position
            {
                get
                {
                    using (locker.Lock(cts.Token))
                    {
                        return _stream.Position;
                    }
                }
                set
                {
                    using (locker.Lock(cts.Token))
                    {
                        _stream.Position = value;
                    }
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return _stream.ReadTimeout;
                }
                set
                {
                    _stream.ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return _stream.WriteTimeout;
                }
                set
                {
                    _stream.WriteTimeout = value;
                }
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    // Explicitly pick up a potentially methodimpl'ed Dispose
                    if (disposing)
                    {
                        cts.Cancel();
                        using (locker.Lock())
                        {
                            ((IDisposable)_stream).Dispose();
                        }
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                using (locker.Lock(cts.Token))
                    return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                using (locker.Lock(cts.Token))
                    _stream.SetLength(value);
            }

            protected override async Task<int> DoReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, bool sync)
            {
                var read = 0;
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                try
                {
                    using (sync ? locker.Lock(linkedCts.Token) : await locker.LockAsync(linkedCts.Token).ConfigureAwait(false))
                    {
                        read = await _stream.DoReadAsync(buffer, offset, count, linkedCts.Token, sync);
                    }
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                return read;
            }

            protected override async Task DoWriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, bool sync)
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                try
                {
                    using (sync ? locker.Lock(linkedCts.Token) : await locker.LockAsync(linkedCts.Token).ConfigureAwait(false))
                    {
                        await _stream.DoWriteAsync(buffer, offset, count, linkedCts.Token, sync);
                    }
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            protected override async Task DoFlushAsync(CancellationToken cancellationToken, bool sync)
            {
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                try
                {
                    using (sync ? locker.Lock(linkedCts.Token) : await locker.LockAsync(linkedCts.Token).ConfigureAwait(false))
                    {
                        await _stream.DoFlushAsync(linkedCts.Token, sync);
                    }
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }
    }

    /// <summary>
    /// AsyncStream class extensions
    /// </summary>
    public static class AsyncStreamExtensions
    {
        /// <summary>
        /// Synchronized version of an async stream
        /// </summary>
        /// <param name="stream">Stream to synchronize</param>
        /// <returns></returns>
        public static AsyncStream Synchronized(this AsyncStream stream)
        {
            return AsyncStream.Synchronized(stream);
        }
    }
}