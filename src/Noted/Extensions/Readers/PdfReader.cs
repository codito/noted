// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
    using UglyToad.PdfPig.Geometry;
    using Annotation = Noted.Core.Models.Annotation;
    using AnnotationType = UglyToad.PdfPig.Annotations.AnnotationType;

    public class PdfReader : IDocumentReader
    {
        private static readonly Regex HyphenWordBreak =
            new("\\w\\-$", RegexOptions.Compiled);

        private ILogger logger;

        public PdfReader(ILogger logger)
        {
            this.logger = logger;
        }

        public List<string> SupportedExtensions => new() { "pdf" };

        public Task<Document> Read(
            Stream stream,
            ReaderOptions options,
            Func<DocumentReference, List<Annotation>> fetchExternalAnnotations)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable.", nameof(stream));
            }

            var doc = PdfDocument.Open(stream);
            var docReference = new DocumentReference
            {
                Title = doc.Information.Title,
                Author = doc.Information.Author
            };

            // TODO add support for external annotations in pdf
            var annotations = new List<Annotation>();
            foreach (var page in doc.GetPages())
            {
                var letters = page.Letters;
                var words = NearestNeighbourWordExtractor.Instance
                    .GetWords(letters, GetWordExtractOptions())
                    .ToList();
                foreach (var annotation in page.ExperimentalAccess.GetAnnotations().Where(a => a.Type.Equals(AnnotationType.Highlight)))
                {
                    // Find highlighted words
                    var highlightedWords = new StringBuilder();
                    foreach (var quad in annotation.QuadPoints)
                    {
                        // Quad points are in anti-clockwise direction starting bottomLeft
                        var points = quad.Points;
                        var rect = new PdfRectangle(points[3], points[2], points[0], points[1]);

                        var textInRegion = GetTextInRegion(words, rect, false);

                        // Combine words split due to region
                        highlightedWords.Append(TrimSplitWord(textInRegion));
                    }

                    DateTime.TryParse(
                        annotation.ModifiedDate,
                        out var createdDate);
                    annotations.Add(new Annotation(
                        highlightedWords.ToString(),
                        docReference,
                        Core.Models.AnnotationType.Highlight,
                        new AnnotationContext
                        {
                            Content = GetTextWithContextInRegion(
                                words,
                                annotation.Rectangle)
                        },
                        createdDate));
                }
            }

            DateTime.TryParse(doc.Information.ModifiedDate, out var modifiedDate);
            return Task.FromResult(new Document
            {
                Title = docReference.Title,
                Author = docReference.Author,
                Subject = doc.Information.Subject,
                Keywords = doc.Information.Keywords,
                ModifiedDate = modifiedDate,
                Annotations = annotations
            });
        }

        private static string GetTextInRegion(
            IEnumerable<Word> words,
            PdfRectangle rect,
            bool trimHyphenatedWords = true)
        {
            // Naive method below removes any formatting in the pdf
            var wordsInRegion = words
                .Where(w => rect.Contains(w.BoundingBox.Centroid))
                .Select(w => w.Text);
            var buffer = new StringBuilder();

            foreach (var word in wordsInRegion)
            {
                if (string.IsNullOrEmpty(word) || word == " ")
                {
                    continue;
                }

                // Combine words split due to region
                if (trimHyphenatedWords)
                {
                    buffer.Append(TrimSplitWord(word));
                    continue;
                }

                buffer.AppendFormat("{0} ", word);
            }

            return buffer.ToString().TrimEnd();
        }

        private static string GetTextWithContextInRegion(
            IReadOnlyCollection<Word> words,
            PdfRectangle rect)
        {
            // DocstrumBoundingBoxes algorithm is more capable and generic, however it
            // ends up expanding the text across multiple columns.
            // RecursiveXYCut is much better in handling these cases. It will suit our
            // target scenarios which primarily include papers, articles and book content.
            // Note that the algorithm may end up joining multiple paragraphs if there isn't
            // enough line separation between them.
            var blocks = RecursiveXYCut.Instance.GetBlocks(words);
            var blocksAroundRegion =
                blocks.Where(b => b.BoundingBox.Contains(rect.Centroid)).ToList();

            // Because we're using the highlighted rectangle (not the quads), the centroid
            // may not be in any identified block. Fallback to a good default.
            if (blocksAroundRegion.Count == 0)
            {
                return GetTextInRegion(words, rect);
            }

            return GetTextInRegion(words, blocksAroundRegion.Single().BoundingBox);
        }

        private static NearestNeighbourWordExtractor.NearestNeighbourWordExtractorOptions GetWordExtractOptions()
        {
            // See https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig.DocumentLayoutAnalysis/WordExtractor/NearestNeighbourWordExtractor.cs#L138
            // for the original implementation. We're using a lower max distance between
            // words (0.1 instead of 0.2) to ensure letters are grouped more accurately.
            return
                new()
                {
                    MaximumDistance = (l1, l2) =>
                    {
                        var maxDist = Math.Max(
                            Math.Max(
                                Math.Max(
                                    Math.Max(
                                        Math.Max(
                                            Math.Abs(l1.GlyphRectangle.Width),
                                            Math.Abs(l2.GlyphRectangle.Width)),
                                        Math.Abs(l1.Width)),
                                    Math.Abs(l2.Width)),
                                l1.PointSize),
                            l2.PointSize) * 0.1;

                        if (l1.TextOrientation ==
                            TextOrientation.Other ||
                            l2.TextOrientation == TextOrientation.Other)
                        {
                            return 2.0 * maxDist;
                        }

                        return maxDist;
                    }
                };
        }

        private static string TrimSplitWord(string word)
        {
            return HyphenWordBreak.IsMatch(word) ? word.TrimEnd('-') : $"{word} ";
        }
    }
}