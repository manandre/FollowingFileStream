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

    internal static class TextReaderExtensions
    {
        public static async Task CopyToAsync(this TextReader reader, TextWriter writer, string stopOn)
        {
            string line = string.Empty;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != stopOn)
            {
                await writer.WriteLineAsync(line).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
