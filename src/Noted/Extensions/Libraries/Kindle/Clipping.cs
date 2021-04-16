// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using System;
    using Noted.Core.Models;

    public class Clipping
    {
        public Clipping() => this.PageNumber = -1;

        public string Book { get; set; } = null!;

        public string Author { get; set; } = null!;

        public ClippingType Type { get; set; }

        public int PageNumber { get; set; }

        public LineLocation Location { get; set; }

        public DateTime CreationDate { get; set; }

        public string Content { get; set; } = null!;
    }
}