// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using Noted.Core;
    using Noted.Core.Models;

    public static class ClippingExtensions
    {
        public static Annotation ToAnnotation(this Clipping clipping)
        {
            return new()
            {
                Content = clipping.Content,
                Context = new AnnotationContext
                {
                    Location = LineLocationScheme.ToString(clipping.Location),
                    PageNumber = clipping.PageNumber
                },
                CreatedDate = clipping.CreationDate,
                Document = new DocumentReference
                {
                    Title = clipping.Book,
                    Author = clipping.Author
                },
                Type = MapAnnotationType(clipping.Type)
            };
        }

        private static AnnotationType MapAnnotationType(ClippingType type)
        {
            return type == ClippingType.Highlight
                ? AnnotationType.Highlight
                : AnnotationType.Note;
        }
    }
}