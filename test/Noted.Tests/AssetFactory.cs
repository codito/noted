// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests
{
    using System.IO;

    public class AssetFactory
    {
        public static readonly string TestDataDir = Path.Join(Path.GetTempPath(), "notedtests");

        static AssetFactory()
        {
            EnsureDirectory(TestDataDir);
        }

        public static Stream GetAsset(params string[] fileNameParts)
        {
            return File.OpenRead(Path.Join(fileNameParts));
        }

        public static string GetKindleLibrary() => Path.Join(".", "kindle");

        public static string GetKOReaderLibrary() => Path.Join(".", "koreader");

        public static string GetPdfLibrary() => Path.Join(".", "pdf");

        public static void EnsureDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }
    }
}