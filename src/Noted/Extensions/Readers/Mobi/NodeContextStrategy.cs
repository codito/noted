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

    public class NodeContextStrategy
    {
        public static List<Annotation> AddContext(
            string contentHtml,
            List<Section> sections,
            List<(Range Location, Annotation Annotation)> externalAnnotations,
            ref DateTime createdDate,
            ref DateTime modifiedDate)
        {
            var annotations = new List<Annotation>();
            var content = new HtmlDocument();
            content.LoadHtml(contentHtml);

            var sb = new StringBuilder();
            var nodeIndex = 0;
            var textIndex = 0;
            var sectionIndex = 0;
            var textNodeMap = new Dictionary<int, int>();
            var textSectionMap = new Dictionary<int, Section>();

            // Convert the entire book into a concatenated string. This is necessary to enable
            // search of annotations that span multiple lines.
            // Create two maps:
            //  textIndex -> nodeIndex
            //  textIndex -> sectionIndex
            // This reduces further processing to finding each annotation and mapping it
            var nodes = content.DocumentNode.SelectNodes("//text()");
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

                nodeIndex += 1;
                textIndex += innerText.Length + 1;

                if (sectionIndex < sections.Count && node.LinePosition >
                    sections[sectionIndex].Location)
                {
                    textSectionMap[textIndex] = sections[sectionIndex++];
                }
            }

            var bookText = sb.ToString();
            var startIndex = 0;
            nodeIndex = 0;
            var nodeMapKeys = textNodeMap.Keys.ToArray();
            var sectionMapKeys = textSectionMap.Keys.ToArray();

            createdDate = DateTime.MaxValue;
            modifiedDate = DateTime.MinValue;
            foreach (var pair in externalAnnotations)
            {
                var annotationContent = pair.Annotation.Content.Trim();
                var index = bookText.IndexOf(
                    annotationContent,
                    startIndex,
                    StringComparison.InvariantCulture);

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

                    var startNodeIndex = textNodeMap[nodeMapKeys[startNode.Index]];
                    pair.Annotation.Context.Location = nodes[startNodeIndex].Line +
                        nodes[startNodeIndex].LinePosition;
                    pair.Annotation.Context.Content = context.ToString();

                    // Find headers which contain this text
                    if (sections.Count > 0)
                    {
                        var headerNode = GetLowerBound(
                            sectionMapKeys,
                            0,
                            startIndex);
                        pair.Annotation.Context.Section =
                            textSectionMap[sectionMapKeys[headerNode.Index]];
                    }
                }

                createdDate = pair.Annotation.CreatedDate < createdDate
                    ? pair.Annotation.CreatedDate
                    : createdDate;
                modifiedDate = pair.Annotation.CreatedDate > modifiedDate
                    ? pair.Annotation.CreatedDate
                    : modifiedDate;
                annotations.Add(pair.Annotation);
            }

            return annotations;
        }

        public static List<Annotation> AddContext2(
            string contentHtml,
            List<(Range Location, Annotation Annotation)> externalAnnotations)
        {
            var annotations = new List<Annotation>();
            var content = new HtmlDocument();
            content.LoadHtml(contentHtml);

            var headers = ExtractSections(contentHtml).ToArray();
            var headerIndex = 0;
            var headerTextMap = new Dictionary<int, string>();

            var sb = new StringBuilder();
            var nodeIndex = 0;
            var textIndex = 0;
            var textNodeMap = new Dictionary<int, int>();
            var nodes = content.DocumentNode.SelectNodes("//text()")
                .ToArray();
            foreach (var node in nodes)
            {
                var innerText =
                    HttpUtility.HtmlDecode(node.InnerText.Trim());
                if (innerText.Length == 0)
                {
                    nodeIndex += 1;
                    continue;
                }

                sb.AppendFormat("{0} ", innerText);
                textNodeMap.Add(textIndex, nodeIndex);

                nodeIndex += 1;
                textIndex += innerText.Length + 1;

                // if (headerIndex < headers.Length && node.LinePosition >
                //     headers[headerIndex].LinePosition)
                // {
                //     headerTextMap[textIndex] =
                //         headers[headerIndex++].HeaderText;
                // }
            }

            var bookText = sb.ToString();
            var startIndex = 0;
            nodeIndex = 0;
            var nodeMapKeys = textNodeMap.Keys.ToArray();
            var headerMapKeys = headerTextMap.Keys.ToArray();

            foreach (var pair in externalAnnotations)
            {
                var annotationContent = pair.Annotation.Content.Trim();
                var index = bookText.IndexOf(
                    annotationContent,
                    startIndex,
                    StringComparison.InvariantCulture);

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

                    // Find headers which contain this text
                    if (headers.Length > 0)
                    {
                        var headerNode = GetLowerBound(
                            headerMapKeys,
                            0,
                            startIndex);
                        pair.Annotation.Context.Section = new Section
                        {
                            Title = headerTextMap[
                                headerMapKeys[headerNode.Index]]
                        };
                    }
                }

                annotations.Add(pair.Annotation);
            }

            return annotations;
        }

        public static List<Section> ExtractSections(string contentHtml)
        {
            // Table of Contents for a book are marked with below node
            //   <reference title="Table of Contents" type="toc" filepos=0000000434 />
            // filepos is the byte location in the text
            // Sample TOC
            // <mbp:pagebreak/><p><a></a></p> <div height="2em" align="center"><font size="4"><b>Table of Contents</b></font></div><div height="1em"></div> <div>�<br/></div> <div align="left"><font size="3"><i><a filepos=0000002732 >Epigraph</a></i></font></div> <div align="left"><font size="3"><i><a filepos=0000000305 >Title Page</a></i></font></div> <div align="left"><font size="3"><i>
            // Extract the TOC html between two page breaks
            var bookHtml = new HtmlDocument();
            bookHtml.LoadHtml(contentHtml);

            var refNodes = bookHtml.DocumentNode
                .SelectNodes("//reference");
            var tocNode = refNodes?.FirstOrDefault(n => n.Attributes["type"].Value == "toc");
            if (tocNode == null)
            {
                return new List<Section>();
            }

            var tocFilePos = int.Parse(tocNode.Attributes["filepos"].Value);
            if (tocFilePos > contentHtml.Length)
            {
                // Malformed book: TOC is beyond the book text
                return new List<Section>();
            }

            var tocContentStart = contentHtml.LastIndexOf(
                "<mbp:pagebreak/>",
                tocFilePos,
                StringComparison.Ordinal);
            var tocContentEnd = contentHtml.IndexOf(
                "<mbp:pagebreak/>",
                tocFilePos,
                StringComparison.Ordinal);
            var tocContent = contentHtml.Substring(
                tocContentStart,
                tocContentEnd - tocContentStart);
            var tocContentHtml = new HtmlDocument();
            tocContentHtml.LoadHtml(tocContent);

            return tocContentHtml.DocumentNode
                .SelectNodes("//a")
                .Where(n => n.Attributes.Contains("filepos"))
                .Select(n => new Section
                    {
                        Title = n.InnerText,
                        Location = int.Parse(n.Attributes["filepos"].Value)
                    })
                .ToList();
        }

        private static (int Index, int Value) GetLowerBound(
            int[] array,
            int startIndex,
            int value)
        {
            var index = Array.BinarySearch(
                array,
                startIndex,
                array.Length - startIndex,
                value);

            // If exact value is not found, use bitwise complement to find the
            // index of next largest value
            index = index < 0 ? ~index : index + 1;
            if (index > array.Length)
            {
                index = array.Length - 1;
            }

            return (index - 1, array[index - 1]);
        }
    }
}