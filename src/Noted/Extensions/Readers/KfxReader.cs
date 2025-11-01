// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Ephemerality.Unpack.KFX;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;

    public class KfxReader(ILogger logger) : IDocumentReader
    {
        private readonly ILogger logger = logger;

        public List<string> SupportedExtensions => ["kfx"];

        public Task<DocumentReference> GetMetadata(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<Document> Read(
            Stream stream,
            ReaderOptions options,
            List<Annotation> annotations)
        {
            var kfx = new KfxContainer(stream);
            var docRef = new DocumentReference
            {
                Title = kfx.Title,
                Author = kfx.Author
            };

            Console.WriteLine($"Book: {docRef.Title}");
            var externalAnnotations = annotations
                .Select(a => (
                    Location: LineLocation.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location)
                .ToList();

            var contents = kfx.GetContentChunks()
                .Where(contentChunk => contentChunk.ContentText != null)
                .Select(x => new { x.ContentName, x.ContentText })
                .ToList();

            return Task.FromResult(new Document());
        }
    }
}