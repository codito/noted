// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Platform.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Noted.Core.Platform.IO;

    public class FileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public Stream OpenPathForRead(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public Stream OpenPathForWrite(string filePath)
        {
            return File.OpenWrite(filePath);
        }

        public IEnumerable<string> GetFiles(string path, string searchPattern)
        {
            if (!this.IsDirectory(path))
            {
                return new[] { path };
            }

            var extRegex = new Regex(searchPattern, RegexOptions.Compiled);
            return Directory.EnumerateFiles(
                    path,
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(f => extRegex.IsMatch(Path.GetExtension(f)));
        }
    }
}