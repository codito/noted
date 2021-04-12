// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ephemerality.Unpack.KFX;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Models;

    public class KfxReader : IDocumentReader
    {
        public List<string> SupportedExtensions => new() { "kfx" };

        public Document Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            var kfx = new KfxContainer(stream);
            var docRef = new DocumentReference
            {
                Title = kfx.Title,
                Author = kfx.Author
            };

            Console.WriteLine($"Book: {docRef.Title}");
            var externalAnnotations = fetchExternalAnnotations(docRef)
                .Select(a => (
                    Location: LineLocationScheme.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location, new RangeComparer())
                .ToList();

            var contents = kfx.GetContentChunks()
                .Where(contentChunk => contentChunk.ContentText != null)
                .Select(x => new { x.ContentName, x.ContentText })
                .ToList();

            return new Document();
        }

        private class RangeComparer : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                return x.Start.Value.CompareTo(y.Start.Value);
            }
        }
    }
}