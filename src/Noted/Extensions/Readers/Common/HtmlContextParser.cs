// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers.Common
{
    using System;
    using System.Collections.Generic;
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
            string contentHtml,
            List<DocumentSection> sections,
            IEnumerable<(LineLocation Location, Annotation Annotation)> externalAnnotations)
        {
            var parser = new HtmlParser(new HtmlParserOptions
            {
                IsKeepingSourceReferences = true
            });
            var document = await parser.ParseDocumentAsync(contentHtml);

            var sb = new StringBuilder();
            var nodeIndex = 0;
            var textIndex = 0;
            var sectionIndex = 0;
            var textNodeMap = new Dictionary<int, int>();
            var textSectionMap = new Dictionary<int, DocumentSection>();

            // Convert the entire book into a concatenated string. This is necessary to enable
            // search for annotations that span multiple lines.
            // Create two maps:
            //  textIndex -> nodeIndex
            //  textIndex -> sectionIndex
            // This reduces further processing to finding each annotation and mapping it
            var annotations = new List<Annotation>();
            var nodes = document.Body.SelectNodes("//text()");
            foreach (var node in nodes)
            {
                var innerText = HttpUtility.HtmlDecode(node.Text().Trim());
                if (innerText.Length == 0)
                {
                    nodeIndex += 1;
                    continue;
                }

                sb.AppendFormat("{0} ", innerText);
                textNodeMap.Add(textIndex, nodeIndex);

                // Note that SourceReference.Index is the offset in the actual html.
                // textIndex is an offset in the trimmed html which is a local concept.
                // Externally, only SourceReference.Index must be shared.
                if (sectionIndex < sections.Count &&
                    node.ParentElement.SourceReference.Position.Index > sections[sectionIndex].Location)
                {
                    // Add offsets to _beginning_ of a text segment which is _after_ the
                    // section. It must be beginning of this section.
                    textSectionMap[textIndex] = sections[sectionIndex++];
                }

                nodeIndex += 1;
                textIndex += innerText.Length + 1;
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
                        context.AppendLine(nodes[key].ParentElement.InnerHtml);
                    }

                    var startNodeIndex = textNodeMap[nodeMapKeys[startNode.Index]];
                    annotation.Context.Location = nodes[startNodeIndex].ParentElement.SourceReference.Position.Index;
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