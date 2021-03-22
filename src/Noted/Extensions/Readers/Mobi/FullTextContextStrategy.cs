// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers.Mobi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using HtmlAgilityPack;
    using Noted.Core.Models;

    public class FullTextContextStrategy
    {
        public static List<Annotation> AddContext(
            string contentHtml,
            List<(Range Location, Annotation Annotation)> externalAnnotations)
        {
            var annotations = new List<Annotation>();
            var content = new HtmlDocument();
            content.LoadHtml(contentHtml);
            var nodes = content.DocumentNode.SelectNodes("//text()").ToArray();
            var sb = new StringBuilder();
            foreach (var node in nodes)
            {
                sb.AppendFormat("{0} ", HttpUtility.HtmlDecode(node.InnerText.Trim()));
            }

            var bookText = sb.ToString();
            var startIndex = 0;

            foreach (var pair in externalAnnotations)
            {
                var annotationContent = pair.Annotation.Content.Trim();
                startIndex = bookText.IndexOf(annotationContent, startIndex, StringComparison.InvariantCulture);

                if (startIndex != -1)
                {
                    var start = startIndex < 100 ? 0 : startIndex - 100;
                    var end = startIndex + annotationContent.Length + 100;
                    if (end > bookText.Length)
                    {
                        end = bookText.Length;
                    }

                    pair.Annotation.Context.Content =
                        bookText.Substring(start, end - start);
                }

                annotations.Add(pair.Annotation);
            }

            return annotations;
        }
    }
}