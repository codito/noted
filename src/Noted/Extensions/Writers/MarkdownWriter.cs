// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

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

            foreach (var annotation in document.Annotations)
            {
                writer.WriteLine($"> {annotation.Content}");
                writer.WriteLine($"{annotation.Context.Content}");
                writer.WriteLine();
            }
        }
    }
}