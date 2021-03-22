// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System;

    /// <summary>
    /// Annotation is a marked or highlighted text along with custom notes for a
    /// text within a document.
    /// </summary>
    /// TODO support context with tree of headings e.g. path from root to child
    public record Annotation
    {
        public string Content { get; init; }

        public AnnotationContext Context { get; set; }

        public DateTime CreatedDate { get; set; }

        public DocumentReference Document { get; init; }

        public AnnotationType Type { get; init; }
    }
}