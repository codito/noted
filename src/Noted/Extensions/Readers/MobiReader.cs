// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using HtmlAgilityPack;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Models;

    public class MobiReader : IDocumentReader
    {
        // TODO add support for azw3
        public List<string> SupportedExtensions => new() { "mobi" };

        public Document Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            using var mobiReader = new Roler.Toolkit.File.Mobi.MobiReader(stream);
            var mobi = mobiReader.Read();

            var text = mobi.Text;
            var docRef = new DocumentReference
            {
                Title = mobi.Title,
                Author = mobi.Creator
            };

            var externalAnnotations = fetchExternalAnnotations(docRef)
                .Select(a => (
                    Location: LineLocationScheme.FromString(a.Context.Location),
                    Annotation: a))
                .OrderBy(p => p.Location, new RangeComparer())
                .ToList();

            // var annotations = FullTextContextStrategy.AddContext(
            //     text,
            //     externalAnnotations);
            var annotations = NodeContextStrategy.AddContext(
                text,
                externalAnnotations);

            // foreach (var pair in externalAnnotations)
            // {
            //     Console.WriteLine(pair.Annotation.Content);
            //     Console.WriteLine(":::");
            //     Console.WriteLine(pair.Annotation.Context.Content);
            //     Console.WriteLine("------------------");
            // }
            return new()
            {
                Title = docRef.Title,
                Author = docRef.Author,
                Annotations = annotations
            };
        }

        private class RangeComparer : IComparer<Range>
        {
            public int Compare(Range x, Range y)
            {
                return x.Start.Value.CompareTo(y.Start.Value);
            }
        }

        private class FullTextContextStrategy
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

        private class NodeContextStrategy
        {
            public static List<Annotation> AddContext(
                string contentHtml,
                List<(Range Location, Annotation Annotation)> externalAnnotations)
            {
                var annotations = new List<Annotation>();
                var content = new HtmlDocument();
                content.LoadHtml(contentHtml);
                var sb = new StringBuilder();
                var nodeIndex = 0;
                var textIndex = 0;
                var textNodeMap = new Dictionary<int, int>();
                var textNodeMapKeys = new List<int>();
                var nodes = content.DocumentNode.SelectNodes("//text()").ToArray();
                foreach (var node in nodes)
                {
                    var innerText = HttpUtility.HtmlDecode(node.InnerText.Trim());
                    if (innerText.Length == 0)
                    {
                        nodeIndex += 1;
                        continue;
                    }

                    sb.AppendFormat("{0} ", innerText);
                    textNodeMap.Add(textIndex, nodeIndex);
                    textNodeMapKeys.Add(textIndex);

                    nodeIndex += 1;
                    textIndex += innerText.Length + 1;
                }

                var bookText = sb.ToString();
                var startIndex = 0;
                nodeIndex = 0;
                var nodeMapKeys = textNodeMapKeys.ToArray();

                foreach (var pair in externalAnnotations)
                {
                    var annotationContent = pair.Annotation.Content.Trim();
                    var index = bookText.IndexOf(annotationContent, startIndex, StringComparison.InvariantCulture);

                    if (index != -1)
                    {
                        startIndex = index;

                        // Find the nodes which contain this annotation
                        var startNode = GetLowerBound(
                            nodeMapKeys,
                            nodeIndex,
                            startIndex);
                        var endNode = GetLowerBound(
                            nodeMapKeys,
                            startNode.Index,
                            startIndex + annotationContent.Length);
                        nodeIndex = startNode.Index;

                        // Concatenate the nodes
                        var context = new StringBuilder();
                        for (var i = startNode.Index; i <= endNode.Index; i++)
                        {
                            var key = textNodeMap[nodeMapKeys[i]];
                            context.AppendLine(nodes[key].InnerHtml);
                        }

                        pair.Annotation.Context.Content = context.ToString();
                    }

                    annotations.Add(pair.Annotation);
                }

                return annotations;
            }

            private static (int Index, int Value) GetLowerBound(
                int[] array,
                int startIndex,
                int value)
            {
                while (array[startIndex] <= value)
                {
                    startIndex += 1;
                }

                return (startIndex - 1, array[startIndex - 1]);
            }
        }
    }
}