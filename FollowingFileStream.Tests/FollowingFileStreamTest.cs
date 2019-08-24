using Microsoft.VisualStudio.TestTools.UnitTesting;
using FollowingFileStream;
using System;
using System.IO;
using System.Threading;

namespace FollowingFileStream.Tests
{
    [TestClass]
    public class FollowingFileStreamTest
    {
        private string inputFilePath = Path.GetTempFileName();
        private string outputFilePath = Path.GetTempFileName();

        public FollowingFileStreamTest()
        {
            using (var sw = File.CreateText(inputFilePath))
            {
                sw.WriteLine("coucou");
            }
        }

        [TestMethod]
        public void FFS_NullPath()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                new FollowingFileStream(null),
                "exception expected on null path"
            );
        }

        [TestMethod]
        public void FFS_EmptyPath()
        {
            Assert.ThrowsException<ArgumentException>(() =>
                new FollowingFileStream(""),
                "exception expected on empty path"
            );
        }

        [TestMethod]
        public void FFS_InvalidPath()
        {
            Assert.ThrowsException<FileNotFoundException>(() =>
                new FollowingFileStream("invalid"),
                "exception expected on invalid path"
            );
        }

        [TestMethod]
        public void FFS_ValidPath()
        {
            using (var ffs = new FollowingFileStream(inputFilePath))
            {
                Assert.IsNotNull(
                    ffs,
                    "stream must not be null on valid path"
                );
            }
        }

        [TestMethod]
        public void FFS_Caps()
        {
            using (var ffs = new FollowingFileStream(inputFilePath))
            {
                Assert.IsTrue(ffs.CanRead);
                Assert.IsFalse(ffs.CanWrite);
                Assert.IsTrue(ffs.CanSeek);
                Assert.IsFalse(ffs.CanTimeout);
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        [TestMethod]
        public void FFS_Properties(bool async)
        {
            using (var ffs = new FollowingFileStream(inputFilePath, 4*1096, async))
            {
                Assert.AreEqual(inputFilePath, ffs.Name);
                Assert.AreEqual(async, ffs.IsAsync);
            }
        }

        [TestMethod]
        public void FFS_Modification()
        {
            using (var ffs = new FollowingFileStream(inputFilePath))
            {
                Assert.ThrowsException<NotSupportedException>(() => ffs.Write(null, 0, 0));
                Assert.ThrowsException<NotSupportedException>(() => ffs.WriteAsync(null, 0, 0));
                Assert.ThrowsException<NotSupportedException>(() => ffs.WriteByte(0x0));
                Assert.ThrowsException<NotSupportedException>(() => ffs.BeginWrite(null, 0, 0, null, null));

                Assert.ThrowsException<NotSupportedException>(() => ffs.SetLength(0));
                Assert.ThrowsException<NotSupportedException>(() => ffs.Flush());
                Assert.ThrowsException<NotSupportedException>(() => ffs.FlushAsync().GetAwaiter().GetResult());
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFS_Read(bool async)
        {
            using (var ffs = new FollowingFileStream(inputFilePath, 4*1096, async))
            {
                Assert.AreEqual(0, ffs.Position);
                Assert.AreEqual(8, ffs.Length);

                var expected = "coucou" + Environment.NewLine;
                var bytes = new byte[8];
                Assert.AreEqual(expected.Length, ffs.Read(bytes, 0, bytes.Length));
                Assert.AreEqual(expected, System.Text.Encoding.Default.GetString(bytes));

                ffs.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(expected.Length, ffs.ReadAsync(bytes, 0, bytes.Length).Result);
                Assert.AreEqual(expected, System.Text.Encoding.Default.GetString(bytes));

                ffs.Position = 0;
                Assert.AreEqual(expected.Length, ffs.EndRead(ffs.BeginRead(bytes, 0, bytes.Length, null, null)));
                Assert.AreEqual(expected, System.Text.Encoding.Default.GetString(bytes));

                var cts = new CancellationTokenSource();
                cts.Cancel();
                Assert.ThrowsExceptionAsync<OperationCanceledException>(() => ffs.ReadAsync(bytes, 0, bytes.Length, cts.Token));
            }
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFS_CopyTo(bool async)
        {
            using (var ffs = new FollowingFileStream(inputFilePath, 4*1096, async))
            using (var destination = File.CreateText(outputFilePath))
            {
                ffs.CopyTo(destination.BaseStream);
            }
            Assert.IsTrue(FileCompare(inputFilePath, outputFilePath));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFS_FollowingRead(bool async)
        {
            using (var input = File.CreateText(inputFilePath))
            using (var ffs = new FollowingFileStream(inputFilePath, 4*1024, async))
            using (var destination = File.CreateText(outputFilePath))
            {
                destination.AutoFlush = true;
                var os = destination.BaseStream;
                var copy = ffs.CopyToAsync(os);
                Assert.AreEqual(0, os.Length);
                Thread.Sleep(200);
                Assert.IsFalse(copy.IsCompleted);
                input.WriteLine("coucou2");
                input.Close();
                Thread.Sleep(200);
                Assert.IsTrue(copy.IsCompletedSuccessfully);
            }
            Assert.IsTrue(FileCompare(inputFilePath, outputFilePath));
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFS_Close(bool async)
        {
            using (var input = File.CreateText(inputFilePath))
            using (var ffs = new FollowingFileStream(inputFilePath, 4*1024, async))
            using (var destination = File.CreateText(outputFilePath))
            {
                destination.AutoFlush = true;
                var os = destination.BaseStream;
                var copy = ffs.CopyToAsync(os);
                Assert.AreEqual(0, os.Length);
                Thread.Sleep(200);
                Assert.IsFalse(copy.IsCompleted);
                input.WriteLine("coucou2");
                ffs.Close();
                Thread.Sleep(200);
                Assert.IsTrue(copy.IsCompletedSuccessfully);
            }
        }

        private bool FileCompare(string file1, string file2)
        {
            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            using (var fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read))
            using (var fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read))
            {
                // Check the file sizes. If they are not the same, the files 
                // are not the same.
                if (fs1.Length != fs2.Length)
                {
                    // Return false to indicate files are different
                    return false;
                }

                // Read and compare a byte from each file until either a
                // non-matching set of bytes is found or until the end of
                // file1 is reached.
                int file1byte;
                int file2byte;
                do
                {
                    // Read one byte from each file.
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));


                // Return the success of the comparison. "file1byte" is 
                // equal to "file2byte" at this point only if the files are 
                // the same.
                return ((file1byte - file2byte) == 0);
            }
        }
    }
}