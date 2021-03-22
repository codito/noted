// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    public class AnnotationContext
    {
        /// <summary>
        /// Gets or sets content block surrounding the annotation.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets document section for this annotation.
        /// </summary>
        public Section Section { get; set; }

        /// <summary>
        /// Gets or sets the annotation location relative to beginning of Document.
        /// </summary>
        public int Location { get; set; }

        /// <summary>
        /// Gets or sets serialized location of the annotation.
        /// </summary>
        public string SerializedLocation { get; set; }

        /// <summary>
        /// Gets or sets page containing the annotation.
        /// </summary>
        public int PageNumber { get; set; }
    }
}