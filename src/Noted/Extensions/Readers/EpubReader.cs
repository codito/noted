// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Html.Parser;
    using AngleSharp.XPath;
    using MiscUtil.Xml.Linq.Extensions;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using VersOne.Epub;
    using Document = Noted.Core.Models.Document;

    public class EpubReader(ILogger logger) : IDocumentReader
    {
        private readonly ILogger logger = logger;

        public List<string> SupportedExtensions => new() { "epub" };

        public async Task<DocumentReference> GetMetadata(Stream stream)
        {
            var epub = await VersOne.Epub.EpubReader.ReadBookAsync(stream);
            var docRef = new DocumentReference
            {
                Title = epub.Title,
                Author = epub.Author,
            };

            return docRef;
        }

        public async Task<Document> Read(
            Stream stream,
            ReaderOptions options,
            List<Annotation> annotations)
        {
            var epub = await VersOne.Epub.EpubReader.ReadBookAsync(stream);
            var docRef = new DocumentReference
            {
                Title = epub.Title,
                Author = epub.Author
            };

            var externalAnnotations = annotations
                .Select(a => (
                    Location: EpubXPathLocation.FromString(a.Context.SerializedLocation),
                    Annotation: a))
                .OrderBy(p => p.Location.Start.DocumentFragmentId)
                .ToList();
            if (externalAnnotations.Count == 0)
            {
                return new Document { Title = docRef.Title, Author = docRef.Author };
            }

            var sections = ParseNavigation(epub);
            var content = new Dictionary<int, string>();
            var parser = new HtmlParser(new HtmlParserOptions
            {
                IsKeepingSourceReferences = true
            });
            var annotationIndex = 0;
            foreach (var annotationTuple in externalAnnotations)
            {
                var docIndex = annotationTuple.Location.Start.DocumentFragmentId - 1;
                var document = await parser.ParseDocumentAsync(epub.ReadingOrder[docIndex].Content);

                var annotation = annotationTuple.Annotation;
                var allNodesInDocument = document.Body.SelectNodes("//*");
                var startNode = document.Body.SelectSingleNode($"/{annotationTuple.Location.Start.XPath}");
                var endNode = document.Body.SelectSingleNode($"/{annotationTuple.Location.End.XPath}");

                // Note we're updating the annotation objects directly in this section.
                var context = GetContext(allNodesInDocument, startNode, endNode);
                annotation.Context.DocumentSection = GetSectionForAnnotation(epub, sections, docIndex, annotation.Context.DocumentSection!.Title, startNode);
                annotation.Context.Location = ((docIndex - 1) * 1000) +
                    context.Item1 == -1 ? annotationIndex : context.Item1;
                annotation.Context.Content = context.Item2;

                annotationIndex++;
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
            var startSelector = start?.ParentElement?.GetSelector();
            var endSelector = end?.ParentElement?.GetSelector();
            if (startSelector == null || endSelector == null)
            {
                return new(-1, string.Empty);
            }

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

        private static DocumentSection GetSectionForAnnotation(EpubBook epub, Dictionary<string, DocumentSection> sections, int docFragmentId, string title, INode startPath)
        {
            // Strategy 1: locate the section by title
            // For older epub documents which don't have well formatted toc navigation.
            if (startPath == null)
            {
                return sections.Where(s => s.Key.Contains(title)).FirstOrDefault().Value;
            }

            // Strategy 2: locate the section by anchor from nav element
            // For epub 3 etc. with well formatted nav.
            foreach (var anc in startPath.GetAncestors())
            {
                var sectionAnchor = (anc as IElement)?.Id ?? string.Empty;
                var key = $"{epub.ReadingOrder[docFragmentId].FilePath}-{sectionAnchor}-{title ?? string.Empty}";

                if (sections.TryGetValue(key, out var section))
                {
                    return section;
                }
            }

            // Strategy 3: fallback to title match.
            if (!string.IsNullOrEmpty(title))
            {
                return sections.Where(s => s.Key.Contains(title)).FirstOrDefault().Value;
            }

            return null!;
        }

        private static void NavigationDfs(
            EpubNavigationItem root,
            Dictionary<string, DocumentSection> result,
            DocumentSection parent,
            int level,
            ref int index)
        {
            // Skip adding HEADER type nodes as toc
            var rootSection = root.Type == EpubNavigationItemType.HEADER ? null
                : new DocumentSection(root.Title, level, ++index * 1000, parent);
            if (rootSection != null)
            {
                // Using an unique combination because the docFragmentId and title can be unique
                // in a specific book. Just title may not be unique.
                // For example: text/foo.xhtml-II
                result.Add($"{root.Link?.ContentFilePath ?? string.Empty}-{root.Link?.Anchor ?? string.Empty}-{root.Title}", rootSection);
            }

            foreach (var nestedItem in root.NestedItems)
            {
                NavigationDfs(nestedItem, result, rootSection!, level + 1, ref index);
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