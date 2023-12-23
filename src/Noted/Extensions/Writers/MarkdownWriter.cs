// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Writers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

            var sectionHeaderPrinted = new HashSet<DocumentSection>();
            foreach (var annotationGroup in document.Annotations.GroupBy(a => a.Context.DocumentSection).OrderBy(g => g.Key?.Location))
            {
                // Print section header
                if (configuration.ExtractDocumentSections)
                {
                    await PrintSectionHeader(configuration, writer, annotationGroup.Key!, sectionHeaderPrinted);
                }

                var annotations = annotationGroup.OrderBy(g => g.Context.Location);
                var currentPage = 0;
                foreach (var annotation in annotations)
                {
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

                    // Ensure newlines are replaced with appropriate new line inside quoted markdown text.
                    await writer.WriteLineAsync($"{prefix} {annotation.Content.Replace("\n", "  \n> ")}");
                    await writer.WriteLineAsync();

                    if (configuration.ExtractionContextLength > 0 && !string.IsNullOrEmpty(annotation.Context.Content))
                    {
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync($"Context: {annotation.Context.Content}");
                    }
                }
            }
        }

        private static async Task PrintSectionHeader(
            Configuration configuration,
            StreamWriter writer,
            DocumentSection section,
            HashSet<DocumentSection> visited)
        {
            var headers = new List<DocumentSection>();
            var sectionPrint = section;
            while (sectionPrint != null)
            {
                if (!visited.Contains(sectionPrint))
                {
                    headers.Insert(0, sectionPrint);
                    visited.Add(sectionPrint);
                }

                sectionPrint = sectionPrint.Parent;
            }

            foreach (var header in headers)
            {
                var (title, level, location, _) = header;
                await writer.WriteLineAsync(
                    $"{new string('#', level)} {title}");
                if (configuration.Verbose)
                {
                    await writer.WriteLineAsync($"<!-- Location: {location} -->");
                }

                await writer.WriteLineAsync();
            }
        }
    }
}