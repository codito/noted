// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System;
    using System.Collections.Generic;

    public record Document : DocumentReference
    {
        public string Subject { get; set; }

        public string Keywords { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string Source { get; set; }

        /// <summary>
        /// Sorted list of annotations in the Document.
        /// </summary>
        public IEnumerable<Annotation> Annotations { get; set; }

        /// <summary>
        /// Sorted list of section headings in the Document.
        /// </summary>
        public IEnumerable<Section> Sections { get; set; }
    }
}