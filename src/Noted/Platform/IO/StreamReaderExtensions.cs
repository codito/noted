// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Platform.IO
{
    using System.Collections.Generic;
    using System.IO;

    public static class StreamReaderExtensions
    {
        public static IEnumerable<string> AsEnumerable(this StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }
    }
}