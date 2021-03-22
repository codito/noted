// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Writers
{
    using System.IO;
    using System.Text;
    using Noted.Core.Extensions;
    using Noted.Core.Models;

    public class MarkdownWriter : IDocumentWriter
    {
        public void Write(Document document, Stream output)
        {
            var writer = new StreamWriter(output, Encoding.UTF8)
                { AutoFlush = true };

            writer.WriteLine("---");
            writer.WriteLine($"title: {document.Title}");
            writer.WriteLine($"author: {document.Author}");
            writer.WriteLine($"start date: {document.CreatedDate}");
            writer.WriteLine($"end date: {document.ModifiedDate}");
            writer.WriteLine("---");
            writer.WriteLine();

            var currentPage = 0;
            using var sectionIterator = document.Sections.GetEnumerator();
            sectionIterator.MoveNext();
            foreach (var annotation in document.Annotations)
            {
                // Print section header
                while (sectionIterator.Current != null &&
                       sectionIterator.Current.Location <= annotation.Context.Location)
                {
                    writer.WriteLine($"## {sectionIterator.Current.Title}");
                    writer.WriteLine();
                    sectionIterator.MoveNext();
                }

                // Print page number
                if (currentPage < annotation.Context.PageNumber)
                {
                    currentPage = annotation.Context.PageNumber;
                    writer.WriteLine($"**Page {currentPage}**");
                    writer.WriteLine();
                }

                var prefix =
                    annotation.Type.Equals(AnnotationType.Highlight)
                        ? ">"
                        : "Note:";
                writer.WriteLine($"{prefix} {annotation.Content}");

                if (!string.IsNullOrEmpty(annotation.Context.Content))
                {
                    writer.WriteLine($"Context: {annotation.Context.Content}");
                }

                writer.WriteLine();
            }
        }
    }
}