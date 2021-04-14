// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System;

    /// <summary>
    /// Location scheme defines the rules to locate content in a document.
    /// E.g. Kindle uses a line based location scheme.
    /// </summary>
    public readonly struct LineLocation : IComparable
    {
        public LineLocation(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        public int Start { get; }

        public int End { get; }

        public static string ToString(LineLocation location)
        {
            return $"line://{location.Start}-{location.End}";
        }

        public static LineLocation FromString(string location)
        {
            var range = new Uri(location).Host.Split('-');
            return new LineLocation(int.Parse(range[0]), int.Parse(range[1]));
        }

        public int CompareTo(object obj)
        {
            if (!(obj is LineLocation other))
            {
                throw new ArgumentException(nameof(obj));
            }

            var startCompare = this.Start.CompareTo(other.Start);
            return startCompare == 0 ? this.End.CompareTo(other.End) : startCompare;
        }
    }
}