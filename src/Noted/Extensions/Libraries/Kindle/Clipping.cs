// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using System;

    public class Clipping
    {
        public Clipping() => this.PageNumber = -1;

        public string Book { get; set; }

        public string Author { get; set; }

        public ClippingType Type { get; set; }

        public int PageNumber { get; set; }

        public Range Location { get; set; }

        public DateTime CreationDate { get; set; }

        public string Content { get; set; }
    }
}