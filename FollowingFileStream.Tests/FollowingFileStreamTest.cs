// --------------------------------------------------------------------------------------------------
// <copyright file="FollowingFileStreamTest.cs" company="Manandre">
// Copyright (c) Manandre. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace Manandre.IO
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test class for FollowingFileStream.
    /// </summary>
    [TestClass]
    public class FollowingFileStreamTest
    {
        private string inputFilePath = Path.GetTempFileName();
        private string outputFilePath = Path.GetTempFileName();

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingFileStreamTest"/> class.
        /// </summary>
        public FollowingFileStreamTest()
        {
            using var sw = File.CreateText(this.inputFilePath);
            sw.Write("coucou");
        }

        /// <summary>
        /// Test FollowingFileStream with null path.
        /// </summary>
        [TestMethod]
        public void FFSNullPath()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new FollowingFileStream(null),
                "exception expected on null path");
        }

        /// <summary>
        /// Test FollowingFileStream with empty path.
        /// </summary>
        [TestMethod]
        public void FFSEmptyPath()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new FollowingFileStream(string.Empty),
                "exception expected on empty path");
        }

        /// <summary>
        /// Test FollowingFileStream with invalid path.
        /// </summary>
        [TestMethod]
        public void FFSInvalidPath()
        {
            Assert.ThrowsException<FileNotFoundException>(
                () => new FollowingFileStream("invalid"),
                "exception expected on invalid path");
        }

        /// <summary>
        /// Test FollowingFileStream with valid path.
        /// </summary>
        [TestMethod]
        public void FFSValidPath()
        {
            using var ffs = new FollowingFileStream(this.inputFilePath);
            Assert.IsNotNull(
                ffs,
                "stream must not be null on valid path");
        }

        /// <summary>
        /// Test FollowingFileStream capabilities.
        /// </summary>
        [TestMethod]
        public void FFSCaps()
        {
            using var ffs = new FollowingFileStream(this.inputFilePath);
            Assert.IsTrue(ffs.CanRead);
            Assert.IsFalse(ffs.CanWrite);
            Assert.IsTrue(ffs.CanSeek);
            Assert.IsTrue(ffs.CanTimeout);
        }

        /// <summary>
        /// Test FollowingFileStream properties.
        /// </summary>
        /// <param name="async">Asynchronous flag.</param>
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFSProperties(bool async)
        {
            using var ffs = new FollowingFileStream(this.inputFilePath, 4 * 1096, async);
            Assert.AreEqual(this.inputFilePath, ffs.Name);
            Assert.AreEqual(async, ffs.IsAsync);
        }

        /// <summary>
        /// Test FollowingFileStream modification methods.
        /// </summary>
        [TestMethod]
        public void FFSModification()
        {
            using var ffs = new FollowingFileStream(this.inputFilePath);
            Assert.ThrowsException<NotSupportedException>(() => ffs.Write(null, 0, 0));
            Assert.ThrowsException<NotSupportedException>(() => ffs.WriteAsync(null, 0, 0));
            Assert.ThrowsException<NotSupportedException>(() => ffs.WriteByte(0x0));
            Assert.ThrowsException<NotSupportedException>(() => ffs.BeginWrite(null, 0, 0, null, null));

            Assert.ThrowsException<NotSupportedException>(() => ffs.SetLength(0));
            Assert.ThrowsException<NotSupportedException>(() => ffs.Flush());
            Assert.ThrowsException<NotSupportedException>(() => ffs.FlushAsync().GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test FollowingFileStream read operations.
        /// </summary>
        /// <param name="async">Asynchronous flag.</param>
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFSRead(bool async)
        {
            using var ffs = new FollowingFileStream(this.inputFilePath, 4 * 1096, async);
            var expected = "coucou";
            Assert.AreEqual(0, ffs.Position);
            Assert.AreEqual(expected.Length, ffs.Length);

            var bytes = new byte[expected.Length];
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
            cts.Dispose();
        }

        /// <summary>
        /// Test FollowingFileStream copy operations.
        /// </summary>
        /// <param name="async">Asynchronous flag.</param>
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFSCopyTo(bool async)
        {
            {
                using var ffs = new FollowingFileStream(this.inputFilePath, 4 * 1096, async);
                using var destination = File.CreateText(this.outputFilePath);
                ffs.ReadTimeout = 0;
                ffs.CopyTo(destination.BaseStream);
            }

            Assert.IsTrue(this.FileCompare(this.inputFilePath, this.outputFilePath));
        }

        /// <summary>
        /// Test FollowingFileStream following read operations.
        /// </summary>
        /// <param name="async">Asynchronous flag.</param>
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFSFollowingRead(bool async)
        {
            {
                using var input = File.CreateText(this.inputFilePath);
                using var ffs = new FollowingFileStream(this.inputFilePath, 4 * 1024, async);
                using var destination = File.CreateText(this.outputFilePath);
                ffs.ReadTimeout = 400;
                destination.AutoFlush = true;
                var os = destination.BaseStream;
                var copy = ffs.CopyToAsync(os);
                Assert.AreEqual(0, os.Length);
                Thread.Sleep(ffs.ReadTimeout / 2);
                Assert.IsFalse(copy.IsCompleted);
                input.WriteLine("coucou2");
                input.Close();
                Assert.IsTrue(copy.Wait(3 * ffs.ReadTimeout));
            }

            Assert.IsTrue(this.FileCompare(this.inputFilePath, this.outputFilePath));
        }

        /// <summary>
        /// Test FollowingFileStream close operations.
        /// </summary>
        /// <param name="async">Asynchronous flag.</param>
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FFSClose(bool async)
        {
            using var input = File.CreateText(this.inputFilePath);
            using var ffsa = new FollowingFileStream(this.inputFilePath, 4 * 1024, async);
            using var ffs = ffsa.Synchronized();
            using var destination = File.CreateText(this.outputFilePath);
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

        private bool FileCompare(string file1, string file2)
        {
            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            using var fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            using var fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

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
            return (file1byte - file2byte) == 0;
        }
    }
}