// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;
    using AngleSharp.XPath;
    using Noted.Core.Models;

    public class HtmlContextParser
    {
        public async Task<(List<Annotation> Annotations, DateTime CreatedDate, DateTime ModifiedDate)> AddContext(
            Stream stream,
            List<DocumentSection> sections,
            IEnumerable<(LineLocation Location, Annotation Annotation)> externalAnnotations)
        {
            var parser = new HtmlParser(new HtmlParserOptions
            {
                IsKeepingSourceReferences = true
            });

            var sb = new StringBuilder();
            var nodeIndex = 0;
            var textIndex = 0;
            var textNodeMap = new Dictionary<int, int>();
            var textSectionMap = new Dictionary<int, DocumentSection>();

            // Convert the entire book into a concatenated string. This is necessary to enable
            // search for annotations that span multiple lines.
            // Create two maps:
            //  textIndex -> nodeIndex
            //  textIndex -> sectionIndex
            // This reduces further processing to finding each annotation and mapping it
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            var annotations = new List<Annotation>();

            // Store html nodes in the book along with their location. Location
            // is section begin offset + node offset in section html content
            var nodes = new List<(INode, int)>();

            // Sort sections because table of contents may have an entry which
            // could point to a location that's already parsed. E.g. entry4 can
            // point to offset that's between entry1 and entry2
            sections.Sort((a, b) => a.Location.CompareTo(b.Location));
            for (var ix = 0; ix < sections.Count; ix++)
            {
                // Parse the html for current section
                var section = sections[ix];
                reader.BaseStream.Seek(section.Location, SeekOrigin.Begin);
                var length = ix + 1 < sections.Count
                    ? sections[ix + 1].Location - section.Location
                    : stream.Length - section.Location;
                var contentHtml = Encoding.UTF8.GetString(reader.ReadBytes((int)length));
                var document = await parser.ParseDocumentAsync(contentHtml);

                var sectionNodes = document.Body.SelectNodes("//text()");
                foreach (var node in sectionNodes)
                {
                    var innerText = HttpUtility.HtmlDecode(node.Text().Trim());
                    if (innerText.Length == 0)
                    {
                        // Skip any empty nodes since they can never have an annotation
                        continue;
                    }

                    sb.AppendFormat("{0} ", innerText);
                    textNodeMap.Add(textIndex, nodeIndex);
                    textSectionMap[textIndex] = section;

                    nodeIndex += 1;
                    textIndex += innerText.Length + 1;

                    // Location of a node is absolute i.e. section start + relative offset
                    // of node in the section
                    nodes.Add((node, section.Location + node.ParentElement?.SourceReference?.Position.Index ?? 0));
                }
            }

            var bookText = sb.ToString();
            var startIndex = 0;
            nodeIndex = 0;
            var nodeMapKeys = textNodeMap.Keys.ToArray();
            var sectionMapKeys = textSectionMap.Keys.ToArray();

            var createdDate = DateTime.MaxValue;
            var modifiedDate = DateTime.MinValue;
            foreach (var (_, annotation) in externalAnnotations)
            {
                var annotationContent = annotation.Content.Trim();
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
                        context.AppendLine(nodes[key].Item1.ParentElement?.InnerHtml);
                    }

                    var startNodeIndex = textNodeMap[nodeMapKeys[startNode.Index]];
                    annotation.Context.Location = nodes[startNodeIndex].Item2;
                    annotation.Context.Content = context.ToString();

                    // Find headers which contain this text
                    if (sections.Count > 0)
                    {
                        var headerNode = GetLowerBound(
                            sectionMapKeys,
                            0,
                            startIndex);
                        annotation.Context.DocumentSection =
                            textSectionMap[sectionMapKeys[headerNode.Index]];
                    }
                }

                createdDate = annotation.CreatedDate < createdDate
                    ? annotation.CreatedDate
                    : createdDate;
                modifiedDate = annotation.CreatedDate > modifiedDate
                    ? annotation.CreatedDate
                    : modifiedDate;
                annotations.Add(annotation);
            }

            return (annotations, createdDate, modifiedDate);
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
