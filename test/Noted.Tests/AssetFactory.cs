// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests
{
    using System.IO;

    public class AssetFactory
    {
        public static Stream GetAsset(string fileName)
        {
            return File.OpenRead(fileName);
        }
    }
}