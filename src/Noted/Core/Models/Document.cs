// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System;
    using System.Collections.Generic;

    public class Document : DocumentReference
    {
        public Document()
        {
            this.Annotations = [];
            this.Sections = [];
        }

        public string? Subject { get; set; }

        public string? Keywords { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets a sorted list of annotations in the Document.
        /// </summary>
        public IEnumerable<Annotation> Annotations { get; set; }

        /// <summary>
        /// Gets or sets a sorted list of section headings in the Document.
        /// </summary>
        public IEnumerable<DocumentSection> Sections { get; set; }
    }
}