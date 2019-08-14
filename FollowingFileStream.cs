using System.IO;
using System.Threading;

namespace FollowingFileStream
{
    public class FollowingFileStream : FileStream
    {
        private const int MillisecondsRetryTimeout = 100;

        #region Constructors
        public FollowingFileStream(string path, FileMode mode) : base(path, mode)
        {
        }


        public FollowingFileStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
        {
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) : base(path, mode, access, share, bufferSize)
        {
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
        {
        }

        public FollowingFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : base(path, mode, access, share, bufferSize, options)
        {
        }
        #endregion
        public override bool CanWrite => false;

        public override int Read(byte[] array, int offset, int count)
        {
            int read = 0;
            do {
                read = base.Read(array, offset, count);
            } while (read == 0 && RetryNeeded());
            return read;
        }

        private bool RetryNeeded()
        {
            bool retry = IsFileLockedForWriting();
            if (retry) {
                Thread.Sleep(MillisecondsRetryTimeout);
            }
            return retry;
        }

        private bool IsFileLockedForWriting()
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(base.Name, FileMode.Open, FileAccess.Write, FileShare.Read);
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
    }
}