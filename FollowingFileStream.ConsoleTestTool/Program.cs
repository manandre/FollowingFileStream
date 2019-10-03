// --------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Manandre">
// Copyright (c) Manandre. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace Manandre.IO
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Console test tool.
    /// </summary>
    internal static class Program
    {
        private const string InputPath = "source.txt";
        private const string OutputPath = "destination.txt";

        /// <summary>
        /// Main routine.
        /// </summary>
        /// <returns>A task representing the main operation.</returns>
        public static async Task Main()
        {
            var input = WriteInput().ConfigureAwait(false);
            await CopyToOutput().ConfigureAwait(false);
            await input;
        }

        private static async Task WriteInput()
        {
            using var sw = new StreamWriter(new FileStream(InputPath, FileMode.Create, FileAccess.Write, FileShare.Read));
            using var sr = new StreamReader(Console.OpenStandardInput());
            await sr.CopyToAsync(sw, stopOn: "quit").ConfigureAwait(false);
        }

        private static async Task CopyToOutput()
        {
            using var source = new FollowingFileStream(InputPath);
            using var destination = new FileStream(OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            await source.CopyToAsync(destination).ConfigureAwait(false);
        }
    }
}