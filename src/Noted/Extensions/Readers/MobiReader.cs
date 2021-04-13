// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ephemerality.Unpack.Mobi;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Extensions.Readers.Mobi;
    using Noted.Extensions.Readers.Navigation;

    public class MobiReader : IDocumentReader
    {
        // TODO add support for azw3
        public List<string> SupportedExtensions => new() { "mobi" };

        public Document Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            var mobi = new MobiMetadata(stream);
            var text = new StreamReader(mobi.GetRawMlStream()).ReadToEnd();
            var docRef = new DocumentReference
            {
                Title = mobi.Title,
                Author = mobi.Author
            };

            Console.WriteLine($"Book: {docRef.Title}");
            var externalAnnotations = fetchExternalAnnotations(docRef)
                .Select(a => (
                    Location: LineLocationScheme.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location, new RangeComparer())
                .ToList();

            // TODO
            // Table of Contents in AZW3 and MOBI differ in significant ways. MOBI adds
            // a toc as a meta/guide at the end of a book. AZW3 provides navigation information
            // in a ncx stream (parsed similar to a epub)
            // https://wiki.mobileread.com/wiki/NCX
            // var annotations = FullTextContextStrategy.AddContext(
            //     text,
            //     externalAnnotations);
            var tocStream = new Mobi7Parser().GetNavigationStream(mobi.GetRawMlStream()).Result;
            var sections = new HtmlTableOfContentParser()
                .Parse(tocStream)
                .ToListAsync()
                .Result;
            var createdDate = DateTime.UnixEpoch;
            var modifiedDate = DateTime.UnixEpoch;
            var annotations = NodeContextStrategy.AddContext(
                text,
                sections,
                externalAnnotations,
                ref createdDate,
                ref modifiedDate);

            return new Document
            {
                Title = docRef.Title,
                Author = docRef.Author,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate,
                Annotations = annotations,
                Sections = sections
            };
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