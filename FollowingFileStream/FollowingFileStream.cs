using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Manandre.IO
{
    /// <summary>
    /// Provides a System.IO.Stream for following a file being written,
    /// supporting both synchronous and asynchronous read operations.
    /// </summary>
    public class FollowingFileStream : AsyncStream
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
        /// CancellationTokenSource
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

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
            fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        /// <summary>
        /// Initializes a new instance of the FollowingFileStream class with the specified
        /// path, buffer size, and synchronous
        /// or asynchronous state.
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
        #endregion

        /// <summary>
        /// Gets the name of the FollowingFileStream that was passed to the constructor.
        /// </summary>
        /// <value>A string that is the name of the FollowingFileStream.</value>
        public virtual string Name => fileStream.Name;

        /// <summary>
        /// Gets a value indicating whether the FollowingFileStream was opened asynchronously or synchronously.
        /// </summary>
        /// <returns>
        /// true if the FollowongFileStream was opened asynchronously; otherwise, false.
        /// </returns>
        public virtual bool IsAsync => fileStream.IsAsync;

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
        public override long Position { get => fileStream.Position; set => fileStream.Position = value; }

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
            do
            {
                read = await fileStream.ReadAsync(buffer, offset, count, cancellationToken);
            }
            while (read == 0 && await RetryNeededAsync());

            // In case the filestream has been written and closed between the last read operation
            // and the IsFileLockedForWriting() check
            if (read == 0)
            {
                read = await fileStream.ReadAsync(buffer, offset, count, cancellationToken);
            }

            TotalTime = 0;

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
            bool retry = true;
            // File locking for read/write operations is only supported on Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                retry = IsFileLockedForWriting();
            }
            if (retry)
            {
                try
                {
                    var duration = MillisecondsRetryTimeout;
                    await Task.Delay(duration, cts.Token).ConfigureAwait(false);
                    TotalTime += duration;
                    retry = ReadTimeout == Timeout.Infinite || TotalTime < ReadTimeout;
                }
                catch (TaskCanceledException)
                {
                    retry = false;
                }
            }
            return retry;
        }

        private long TotalTime;

        /// <summary>
        /// 
        /// </summary>
        public override bool CanTimeout => true;

        /// <summary>
        /// 
        /// </summary>
        public override int ReadTimeout { get; set; } = Timeout.Infinite;

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
                stream?.Dispose();
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

            if (disposing)
            {
                cts.Cancel();
                fileStream.Dispose();
            }

            disposed = true;
            // Call stream class implementation.
            base.Dispose(disposing);
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Not supported</exception>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point
        /// for offset, using a value of type System.IO.SeekOrigin.</param>
        /// <returns>The new position in the stream.</returns>
        /// <exception cref="System.NotSupportedException">
        /// FollowingFileStream.CanSeek for this stream is false.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error, such as the file being closed, occurred.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Seeking is attempted before the beginning of the stream.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return fileStream.Seek(offset, origin);
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
        /// Asynchronously writes a block of bytes to the file stream.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Not supported</exception>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}