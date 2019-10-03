// --------------------------------------------------------------------------------------------------
// <copyright file="AsyncStreamExtensions.cs" company="Manandre">
// Copyright (c) Manandre. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------

namespace Manandre.IO
{
    /// <summary>
    /// AsyncStream class extensions.
    /// </summary>
    public static class AsyncStreamExtensions
    {
        /// <summary>
        /// Synchronized version of an async stream.
        /// </summary>
        /// <param name="stream">Stream to synchronize.</param>
        /// <returns>the synchronized version of the given stream.</returns>
        public static AsyncStream Synchronized(this AsyncStream stream)
        {
            return AsyncStream.Synchronized(stream);
        }
    }
}