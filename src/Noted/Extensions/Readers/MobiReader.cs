// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Ephemerality.Unpack.Mobi;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using Noted.Extensions.Readers.Common;
    using Noted.Extensions.Readers.Mobi;

    public class MobiReader(ILogger logger) : IDocumentReader
    {
        private readonly ILogger logger = logger;

        // TODO add support for azw3
        public List<string> SupportedExtensions => ["mobi"];

        public Task<DocumentReference> GetMetadata(Stream stream)
        {
            var mobi = new MobiMetadata(stream);
            var docRef = new DocumentReference
            {
                Title = mobi.Title,
                Author = mobi.Author
            };

            return Task.FromResult(docRef);
        }

        public async Task<Document> Read(
            Stream stream,
            ReaderOptions options,
            List<Annotation> annotations)
        {
            var mobi = new MobiMetadata(stream);
            var docRef = new DocumentReference
            {
                Title = mobi.Title,
                Author = mobi.Author
            };

            var externalAnnotations = annotations
                .Select(a => (
                    Location: LineLocation.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location)
                .ToList();

            // TODO
            // Table of Contents in AZW3 and MOBI differ in significant ways. MOBI adds
            // a toc as a meta/guide at the end of a book. AZW3 provides navigation information
            // in a ncx stream (parsed similar to a epub)
            // https://wiki.mobileread.com/wiki/NCX
            // var annotations = FullTextContextStrategy.AddContext(
            //     text,
            //     externalAnnotations);
            var tocStream = Mobi7Parser.GetNavigationStream(mobi.GetRawMlStream()).Result;
            var sections = await HtmlSectionParser
                .Parse(tocStream)
                .OrderBy(s => s.Location)
                .ToListAsync();

            var rawMlStream = mobi.GetRawMlStream();
            var (updatedAnnotations, createdDate, modifiedDate) = await HtmlContextParser.AddContext(
                rawMlStream,
                sections,
                externalAnnotations);

            return new Document
            {
                Title = docRef.Title,
                Author = docRef.Author,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate,
                Annotations = updatedAnnotations,
                Sections = sections
            };
        }
    }
}