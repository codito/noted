// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;
    using AngleSharp.XPath;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using VersOne.Epub;
    using Document = Noted.Core.Models.Document;

    public class EpubReader(ILogger logger) : IDocumentReader
    {
        private readonly ILogger logger = logger;

        public List<string> SupportedExtensions => new() { "epub" };

        public async Task<Document> Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            var epub = await VersOne.Epub.EpubReader.ReadBookAsync(stream);
            var docRef = new DocumentReference
            {
                Title = epub.Title,
                Author = epub.Author
            };
            var annotations = new List<Annotation>();

            var externalAnnotations = fetchExternalAnnotations(docRef)
                .Select(a => (
                    Location: EpubXPathLocation.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location.Start.DocumentFragmentId)
                .ToList();
            var sections = ParseNavigation(epub);
            var content = new Dictionary<int, string>();
            var parser = new HtmlParser(new HtmlParserOptions
            {
                IsKeepingSourceReferences = true
            });
            foreach (var annotationTuple in externalAnnotations)
            {
                var docIndex = annotationTuple.Location.Start.DocumentFragmentId;
                var document = await parser.ParseDocumentAsync(epub.ReadingOrder[docIndex - 1].Content);

                var annotation = annotationTuple.Annotation;
                var allNodesInDocument = document.Body.SelectNodes("//*");
                var startNode = document.Body.SelectSingleNode($"/{annotationTuple.Location.Start.XPath}");
                var endNode = document.Body.SelectSingleNode($"/{annotationTuple.Location.End.XPath}");

                var context = GetContext(allNodesInDocument, startNode, endNode);
                annotation.Context.DocumentSection = sections[annotation.Context.DocumentSection!.Title];
                annotation.Context.Location = ((docIndex - 1) * 1000) + context.Item1;
                annotation.Context.Content = context.Item2;
                annotations.Add(annotation);
            }

            var sortedSections = sections.Values.OrderBy(s => s.Location).ToList();
            return new Document
            {
                Title = docRef.Title,
                Author = docRef.Author,
                Annotations = annotations.OrderBy(a => a.Context.Location),
                Sections = sortedSections
            };
        }

        private static Tuple<int, string> GetContext(List<INode> allNodes, INode start, INode end)
        {
            var startSelector = start.ParentElement!.GetSelector();
            var endSelector = end.ParentElement!.GetSelector();
            var nodesBetween = new List<string>();
            var startLocation = 0;

            // Create context by selecting all nodes between the start and end node selectors.
            // Assumes that allNodes is a depth-first traversal of the entire document.
            var addNode = false;
            var index = 0;
            foreach (var node in allNodes)
            {
                var nodeElementSelector = (node as IElement)?.GetSelector() ?? string.Empty;
                if (nodeElementSelector == startSelector)
                {
                    startLocation = index;
                    addNode = true;
                }

                if (addNode)
                {
                    nodesBetween.Add(node.TextContent);
                }

                index++;

                if (nodeElementSelector == endSelector)
                {
                    break;
                }
            }

            return new(startLocation, string.Join(Environment.NewLine, nodesBetween));
        }

        private static void NavigationDfs(
            EpubNavigationItem root,
            Dictionary<string, DocumentSection> result,
            DocumentSection parent,
            int level,
            ref int index)
        {
            var rootSection = new DocumentSection(root.Title, level, ++index * 1000, parent);
            result.Add(root.Title, rootSection);
            foreach (var nestedItem in root.NestedItems)
            {
                NavigationDfs(nestedItem, result, rootSection, level + 1, ref index);
            }
        }

        private static Dictionary<string, DocumentSection> ParseNavigation(EpubBook epub)
        {
            var result = new Dictionary<string, DocumentSection>();
            if (epub.Navigation == null)
            {
                return result;
            }

            var index = 0;
            foreach (var navItem in epub.Navigation)
            {
                // Level indicates the current branch level of a section.
                // Index is monotonically increasing sequence number used to ensure 1, 1.1.1, 1.1.2, 1.2 ...
                // follow the same order. It is stored as Location of the section.
                NavigationDfs(navItem, result, null!, 1, ref index);
            }

            return result;
        }
    }
}