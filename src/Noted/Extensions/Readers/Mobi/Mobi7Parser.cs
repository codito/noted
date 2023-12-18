// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers.Mobi
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp;

    /// <summary>
    /// MOBI 7 is the older file format used in Kindle readers.
    /// </summary>
    public class Mobi7Parser
    {
        public static async Task<Stream> GetNavigationStream(Stream contentStream)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(contentStream));

            // Table of Contents for a book are marked with below node
            //   <reference title="Table of Contents" type="toc" filepos=0000000434 />
            // filepos is the byte location in the text
            // Sample TOC
            // <mbp:pagebreak/><p><a></a></p> <div height="2em" align="center"><font size="4"><b>Table of Contents</b></font></div><div height="1em"></div> <div>�<br/></div> <div align="left"><font size="3"><i><a filepos=0000002732 >Epigraph</a></i></font></div> <div align="left"><font size="3"><i><a filepos=0000000305 >Title Page</a></i></font></div> <div align="left"><font size="3"><i>
            // Extract the TOC html between two page breaks
            var tocNode = document.QuerySelectorAll("reference")
                .FirstOrDefault(n => n.GetAttribute("type") == "toc");
            if (tocNode == null)
            {
                return Stream.Null;
            }

            var tocFilePos = int.Parse(tocNode.GetAttribute("filepos") ?? "0");
            if (tocFilePos > contentStream.Length)
            {
                // Malformed book: TOC is beyond the book text
                return Stream.Null;
            }

            // Find the end of table of contents by searching for a page break
            contentStream.Seek(tocFilePos, SeekOrigin.Begin);
            using var reader = new StreamReader(contentStream);
            var contentHtml = await reader.ReadToEndAsync();
            var tocContentEnd = contentHtml.IndexOf(
                "<mbp:pagebreak/>",
                StringComparison.Ordinal);

            // Prepare the table of contents stream for return
            contentStream.Seek(tocFilePos, SeekOrigin.Begin);
            var bytes = new byte[tocContentEnd];
            await contentStream.ReadAsync(bytes);
            return new MemoryStream(bytes);
        }
    }
}