// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Writers
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;

    public class MarkdownWriter(ILogger logger) : IDocumentWriter
    {
        private readonly ILogger logger = logger;

        public async Task Write(Configuration configuration, Document document, Stream output)
        {
            var writer = new StreamWriter(output, Encoding.UTF8)
            { AutoFlush = true };

            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync($"title: {document.Title}");
            await writer.WriteLineAsync($"author: {document.Author}");
            await writer.WriteLineAsync($"start date: {document.CreatedDate}");
            await writer.WriteLineAsync($"end date: {document.ModifiedDate}");
            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync();

            var currentPage = 0;
            using var sectionIterator = document.Sections.GetEnumerator();
            foreach (var annotation in document.Annotations)
            {
                // Print section header
                if (configuration.ExtractDocumentSections)
                {
                    while (sectionIterator.MoveNext() &&
                           sectionIterator.Current != null &&
                           sectionIterator.Current.Location <=
                           annotation.Context.Location)
                    {
                        var (title, level, location) = sectionIterator.Current;
                        await writer.WriteLineAsync(
                            $"{new string('#', level)} {title}");
                        if (configuration.Verbose)
                        {
                            await writer.WriteLineAsync($"<!-- Location: {location} -->");
                        }

                        await writer.WriteLineAsync();
                    }
                }

                // Print page number
                if (currentPage < annotation.Context.PageNumber)
                {
                    currentPage = annotation.Context.PageNumber;
                    await writer.WriteLineAsync($"**Page {currentPage}**");
                    if (configuration.Verbose)
                    {
                        var section = annotation.Context.DocumentSection;
                        var location = annotation.Context.Location;
                        await writer.WriteLineAsync($"<!-- Location: {location} -->");
                        await writer.WriteLineAsync($"<!-- Section: {section?.Level} - {section?.Title} at {section?.Location} -->");
                    }

                    await writer.WriteLineAsync();
                }

                var prefix =
                    annotation.Type.Equals(AnnotationType.Highlight)
                        ? ">"
                        : "Note:";
                await writer.WriteLineAsync($"{prefix} {annotation.Content}");

                if (configuration.ExtractionContextLength > 0 && !string.IsNullOrEmpty(annotation.Context.Content))
                {
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync($"Context: {annotation.Context.Content}");
                }

                await writer.WriteLineAsync();
            }
        }
    }
}