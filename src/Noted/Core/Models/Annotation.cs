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
    public record Annotation(
        string Content,
        DocumentReference Document,
        AnnotationType Type,
        AnnotationContext Context,
        DateTime CreatedDate);
}