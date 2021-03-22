// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Platform.IO
{
    using System.Collections.Generic;
    using System.IO;

    public interface IFileSystem
    {
        /// <summary>
        /// Check if the file or directory path exists.
        /// </summary>
        /// <param name="path">File or Directory path.</param>
        /// <returns>True if the path exists.</returns>
        bool Exists(string path);

        /// <summary>
        /// Check if the path is a directory.
        /// </summary>
        /// <param name="path">File or Directory path.</param>
        /// <returns>True if the path is a directory.</returns>
        bool IsDirectory(string path);

        /// <summary>
        /// Opens a readonly stream for the file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Stream with file opened in readonly mode.</returns>
        Stream OpenPathForRead(string filePath);

        /// <summary>
        /// Opens a writeable stream for the file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Stream with file opened in writeable mode.</returns>
        Stream OpenPathForWrite(string filePath);

        /// <summary>
        /// Recursively depth-wise enumerate all files in a directory.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="searchPattern">Search for files matching this pattern.</param>
        /// <returns>Path of files in the directory.</returns>
        IEnumerable<string> GetFiles(string path, string searchPattern);
    }
}