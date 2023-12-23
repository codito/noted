// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers.Common
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AngleSharp;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using Noted.Core.Models;

    public class HtmlSectionParser
    {
        public static async IAsyncEnumerable<DocumentSection> Parse(Stream stream)
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
            var prevLevel = 0;
            DocumentSection prevSection = null!;
            foreach (var node in document.QuerySelectorAll("a"))
            {
                depth = node.GetAncestors().Count();
                var fileOffset = node.GetAttribute("filepos");
                if (!levelSet.TryGetValue(depth, out var level))
                {
                    level = levelSet.Count + 1;
                    levelSet[depth] = level;
                }

                var parent = level == 1 ? null : prevSection;  // assume this node is a child
                var count = prevLevel - level;
                while (prevLevel >= level)
                {
                    // if this node is a sibling instead
                    parent = prevSection?.Parent;
                    prevSection = parent ?? null!;
                    prevLevel--;
                }

                var section = new DocumentSection(
                    node.Text(),
                    level,
                    string.IsNullOrEmpty(fileOffset) ? 0 : int.Parse(fileOffset),
                    parent);

                yield return section;

                prevLevel = level;
                prevSection = section;
            }
        }
    }
}