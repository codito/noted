// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System.Collections.Generic;

    public class AnnotationContext
    {
        /// <summary>
        /// Gets or sets content block surrounding the annotation.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets collection of headings from root to the annotation.
        /// </summary>
        public IReadOnlyCollection<string> Headers { get; set; }

        /// <summary>
        /// Gets or sets serialized location of the annotation.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets page containing the annotation.
        /// </summary>
        public int PageNumber { get; set; }
    }
}