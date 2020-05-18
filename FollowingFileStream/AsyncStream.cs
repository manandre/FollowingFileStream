// --------------------------------------------------------------------------------------------------
// <copyright file="AsyncStream.cs" company="Manandre">
// Copyright (c) Manandre. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace Manandre.IO
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Nito.AsyncEx;
#if !NETSTANDARD1_3
    using Nito.AsyncEx.Interop;
#endif
    using Nito.AsyncEx.Synchronous;

    /// <summary>
    /// AsyncStream class.
    /// </summary>
#pragma warning disable S3881
    public abstract class AsyncStream : Stream
    {
        /// <summary>
        /// Synchronized version of an async stream.
        /// </summary>
        /// <param name="stream">Stream to synchronize.</param>
        /// <returns>synchronized version of the given stream.</returns>
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
        {
            return ApmAsyncFactory.ToBegin(
                this.ReadAsync(buffer, offset, count, CancellationToken.None),
                callback,
                state);
        }

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
        {
            return ApmAsyncFactory.ToBegin(
                this.WriteAsync(buffer, offset, count, CancellationToken.None),
                callback,
                state);
        }

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
        {
            return ApmAsyncFactory.ToEnd<int>(asyncResult);
        }

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
        {
            ApmAsyncFactory.ToEnd(asyncResult);
        }
#endif

        /// <summary>
        /// Clears all buffers for this stream and causes
        /// any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// The stream is closed or an internal error has occurred.
        /// </exception>
        public sealed override void Flush()
        {
            this.FlushAsync().WaitAndUnwrapException();
        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to
        /// be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.
        /// The default value is System.Threading.CancellationToken.None.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public abstract override Task FlushAsync(CancellationToken cancellationToken);

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
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            return this.ReadAsync(buffer, offset, count, CancellationToken.None).WaitAndUnwrapException();
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
        public abstract override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

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
        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            this.WriteAsync(buffer, offset, count, CancellationToken.None).WaitAndUnwrapException();
        }

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
        public abstract override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

#if NETSTANDARD2_1
        /// <summary>
        /// Asynchronously releases all resources used by the AsyncStream.
        /// </summary>
        /// <returns>A ValueTask representing the dispose operation.</returns>
        public sealed override ValueTask DisposeAsync() => this.DisposeAsync(true);

        /// <summary>
        /// Asynchronously releases the unmanaged resources used by the FollowingFileStream and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <returns>A ValueTask representing the dispose operation.</returns>
        protected virtual ValueTask DisposeAsync(bool disposing) => default;

        /// <summary>
        /// Releases the unmanaged resources used by the FollowingFileStream and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected sealed override void Dispose(bool disposing) => this.DisposeAsync(disposing).GetAwaiter().GetResult();
#else
        /// <summary>
        /// Releases the unmanaged resources used by the FollowingFileStream and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Call stream class implementation.
            base.Dispose(disposing);
        }
#endif

        private sealed class AsyncSafeStream : AsyncStream
        {
            private readonly AsyncStream asyncStream;
            private readonly CancellationTokenSource cts = new CancellationTokenSource();
            private readonly AsyncLock locker = new AsyncLock();
            private bool disposed = false;

            public AsyncSafeStream(AsyncStream stream)
            {
                this.asyncStream = stream ?? throw new ArgumentNullException(nameof(stream));
            }

            public override bool CanRead => this.asyncStream.CanRead;

            public override bool CanWrite => this.asyncStream.CanWrite;

            public override bool CanSeek => this.asyncStream.CanSeek;

            public override bool CanTimeout => this.asyncStream.CanTimeout;

            public override long Length
            {
                get
                {
                    using (this.locker.Lock(this.cts.Token))
                    {
                        return this.asyncStream.Length;
                    }
                }
            }

            public override long Position
            {
                get
                {
                    using (this.locker.Lock(this.cts.Token))
                    {
                        return this.asyncStream.Position;
                    }
                }

                set
                {
                    using (this.locker.Lock(this.cts.Token))
                    {
                        this.asyncStream.Position = value;
                    }
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this.asyncStream.ReadTimeout;
                }

                set
                {
                    this.asyncStream.ReadTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this.asyncStream.WriteTimeout;
                }

                set
                {
                    this.asyncStream.WriteTimeout = value;
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                using (this.locker.Lock(this.cts.Token))
                {
                    return this.asyncStream.Seek(offset, origin);
                }
            }

            public override void SetLength(long value)
            {
                using (this.locker.Lock(this.cts.Token))
                {
                    this.asyncStream.SetLength(value);
                }
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var read = 0;
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.cts.Token))
                {
                    try
                    {
                        using (await this.locker.LockAsync(linkedCts.Token))
                        {
                            read = await this.asyncStream.ReadAsync(buffer, offset, count, linkedCts.Token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return read;
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.cts.Token);
                try
                {
                    using (await this.locker.LockAsync(linkedCts.Token))
                    {
                        await this.asyncStream.WriteAsync(buffer, offset, count, linkedCts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            public override async Task FlushAsync(CancellationToken cancellationToken)
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.cts.Token);
                try
                {
                    using (await this.locker.LockAsync(linkedCts.Token))
                    {
                        await this.asyncStream.FlushAsync(linkedCts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

#if NETSTANDARD2_1
            protected override async ValueTask DisposeAsync(bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }

                try
                {
                    // Explicitly pick up a potentially methodimpl'ed DisposeAsync
                    if (disposing)
                    {
                        this.cts.Cancel();
                        using (await this.locker.LockAsync())
                        {
                            await ((IAsyncDisposable)this.asyncStream).DisposeAsync().ConfigureAwait(false);
                        }

                        this.cts.Dispose();
                    }
                }
                finally
                {
                    this.disposed = true;
                    await base.DisposeAsync(disposing).ConfigureAwait(false);
                }
            }
#else
            protected override void Dispose(bool disposing)
            {
                if (this.disposed)
                {
                    return;
                }

                try
                {
                    // Explicitly pick up a potentially methodimpl'ed Dispose
                    if (disposing)
                    {
                        this.cts.Cancel();
                        using (this.locker.Lock())
                        {
                            ((IDisposable)this.asyncStream).Dispose();
                        }

                        this.cts.Dispose();
                    }
                }
                finally
                {
                    this.disposed = true;
                    base.Dispose(disposing);
                }
            }
#endif
        }
    }
#pragma warning restore S3881
}