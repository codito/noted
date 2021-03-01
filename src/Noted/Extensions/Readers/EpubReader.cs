// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using HtmlAgilityPack;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using VersOne.Epub;

    public class EpubReader : IDocumentReader
    {
        public List<string> SupportedExtensions => new() { "epub" };

        public Document Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            var epub = VersOne.Epub.EpubReader.ReadBook(stream);
            var docRef = new DocumentReference
            {
                Title = epub.Title,
                Author = epub.Author
            };

            var externalAnnotations = fetchExternalAnnotations(docRef)
                .Select(a => (
                    Location: LineLocationScheme.FromString(a.Context.Location),
                    Annotation: a))
                .OrderBy(p => p.Location)
                .ToList();

            var content = new Dictionary<int, string>();
            var line = 1;
            foreach (EpubTextContentFile textContentFile in epub.ReadingOrder)
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(textContentFile.Content);
                foreach (var node in htmlDocument.DocumentNode.SelectNodes("//text()"))
                {
                    content.Add(line++, node.InnerText.Trim());
                }
            }

            foreach (var pair in externalAnnotations)
            {
                Console.WriteLine(pair.Annotation.Content);
                Console.WriteLine(":::");
                Console.WriteLine(content[pair.Location.Start.Value]);
                Console.WriteLine("------------------");
            }

            return new()
            {
                Title = docRef.Title,
                Author = docRef.Author,
            };
        }

        private class RangeComparer : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                throw new NotImplementedException();
            }
        }
    }
}