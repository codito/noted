// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp.Html.Parser;
    using AngleSharp.XPath;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using VersOne.Epub;

    public class EpubReader : IDocumentReader
    {
        public List<string> SupportedExtensions => new() { "epub" };

        public async Task<Document> Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            var epub = await VersOne.Epub.EpubReader.ReadBookAsync(stream);
            var docRef = new DocumentReference
            {
                Title = epub.Title,
                Author = epub.Author
            };

            var externalAnnotations = fetchExternalAnnotations(docRef)
                .Select(a => (
                    Location: LineLocation.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location)
                .ToList();

            var content = new Dictionary<int, string>();
            var line = 1;
            foreach (EpubTextContentFile textContentFile in epub.ReadingOrder)
            {
                var parser = new HtmlParser(new HtmlParserOptions
                {
                    IsKeepingSourceReferences = true
                });
                var document = await parser.ParseDocumentAsync(textContentFile.Content);
                foreach (var node in document.Body.SelectNodes("//text()"))
                {
                    content.Add(line++, node.TextContent.Trim());
                }
            }

            foreach (var pair in externalAnnotations)
            {
                Console.WriteLine(pair.Annotation.Content);
                Console.WriteLine(":::");
                Console.WriteLine(content[pair.Location.Start]);
                Console.WriteLine("------------------");
            }

            return new Document
            {
                Title = docRef.Title,
                Author = docRef.Author,
            };
        }
    }
}