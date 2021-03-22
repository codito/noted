// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System;
    using System.Linq;

    /// <summary>
    /// Location scheme defines the rules to locate content in a document.
    /// E.g. Kindle uses a line based location scheme.
    /// </summary>
    public static class LineLocationScheme
    {
        public static string ToString(Range location)
        {
            return $"line://{location.Start.Value}-{location.End.Value}";
        }

        public static Range FromString(string location)
        {
            var range = new Uri(location).Host.Split('-');
            return new Range(
                new(int.Parse(range[0])),
                new(int.Parse(range[1])));
        }
    }
}