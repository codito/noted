// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using Noted.Core.Models;

    public static class ClippingExtensions
    {
        public static Annotation ToAnnotation(this Clipping clipping)
        {
            return new(
                clipping.Content,
                new DocumentReference
                {
                    Title = clipping.Book,
                    Author = clipping.Author
                },
                MapAnnotationType(clipping.Type),
                new AnnotationContext
                {
                    SerializedLocation = LineLocation.ToString(clipping.Location),
                    PageNumber = clipping.PageNumber
                },
                clipping.CreationDate);
        }

        private static AnnotationType MapAnnotationType(ClippingType type)
        {
            return type == ClippingType.Highlight
                ? AnnotationType.Highlight
                : AnnotationType.Note;
        }
    }
}