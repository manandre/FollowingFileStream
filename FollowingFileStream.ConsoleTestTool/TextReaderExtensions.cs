// --------------------------------------------------------------------------------------------------
// <copyright file="TextReaderExtensions.cs" company="Manandre">
// Copyright (c) Manandre. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace Manandre.IO
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for the <see cref="TextReader"/> class.
    /// </summary>
    internal static class TextReaderExtensions
    {
        /// <summary>
        /// Copy lines from reader to writer until a specific string is reached.
        /// </summary>
        /// <param name="reader">A text reader to read from.</param>
        /// <param name="writer">A text writer to write to.</param>
        /// <param name="stopOn">A string on which to stop copying.</param>
        /// <returns>A task representing the copy operation.</returns>
        public static async Task CopyToAsync(this TextReader reader, TextWriter writer, string stopOn)
        {
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != stopOn)
            {
                await writer.WriteLineAsync(line).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
