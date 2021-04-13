// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers.Navigation
{
    using System.Collections.Generic;
    using System.IO;
    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using Noted.Core.Models;

    public class HtmlTableOfContentParser
    {
        public async IAsyncEnumerable<DocumentNavigation> Parse(Stream stream)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(stream));
            var levelSet = new Dictionary<int, int>();

            // Sample toc fragment:
            //  <p><a filepos=1000>level 1</a></p>
            //  <blockquote><blockquote><a filepos=1001>level 1.1</a>...
            //  <blockquote><blockquote><a filepos=1002>level 1.2</a>...
            //  <p><a filepos=2000>level 2</a></p>
            // Note that both parent and child levels share a common root. Our
            // level calculation leverages this.
            var depth = 0;
            foreach (var node in document.All)
            {
                // AngleSharp always inserts the provided fragment within a
                // body element. We reset the depth accordingly.
                // Alternatively, we could do node.GetAncestors().Count but that
                // revisits the parent nodes multiple times.
                depth = node.Parent == document.Body ? 0 : depth + 1;
                if (node is not IHtmlAnchorElement)
                {
                    continue;
                }

                var fileOffset = node.GetAttribute("filepos");
                if (!levelSet.TryGetValue(depth, out var level))
                {
                    level = levelSet.Count + 1;
                    levelSet[depth] = level;
                }

                yield return new DocumentNavigation
                {
                    Title = node.Text(),
                    Level = level,
                    Location = string.IsNullOrEmpty(fileOffset) ? 0 : int.Parse(fileOffset)
                };
            }
        }
    }
}