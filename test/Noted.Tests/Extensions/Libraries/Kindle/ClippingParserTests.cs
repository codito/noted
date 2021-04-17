// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Libraries.Kindle
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Models;
    using Noted.Extensions.Libraries.Kindle;

    [TestClass]
    public class ClippingParserTests : IDisposable
    {
        private readonly MemoryStream stream;
        private readonly StreamWriter writer;
        private readonly Clipping clipping;

        public ClippingParserTests()
        {
            this.stream = new MemoryStream();
            this.writer = new StreamWriter(this.stream, Encoding.Unicode) { AutoFlush = true };
            this.clipping = new Clipping();
        }

        [TestMethod]
        public void ParseShouldThrowIfStreamIsClosed()
        {
            this.stream.Close();

            Assert.ThrowsException<ArgumentException>(() => ClippingParser.Parse(this.stream).ToList());
        }

        [TestMethod]
        public void ParseShouldExtractAllClippings()
        {
            const string sampleClippings =
                @"The Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Highlight on page 145 | Location 3015-3016 | Added on Thursday, August 15, 2019 10:14:40 AM

Forcing functions can be a nuisance in normal usage. The result is that many people will deliberately disable the forcing function, thereby negating its safety feature.
==========
 The Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Note on Location 3026-3027 | Added on Thursday, August 15, 2019 10:16:42 AM

Affordances refer to the potential actions that are possible, but these are easily discoverable only if they are perceivable: perceived affordances. It is the signifier component of the perceived affordance that allows people to determine the possible actions.
==========";
            this.WriteText(sampleClippings);

            var clippings = ClippingParser.Parse(this.stream).ToList();

            Assert.AreEqual(2, clippings.Count);
            Assert.AreEqual("The Design of Everyday Things: Revised and Expanded Edition", clippings[0].Book);
            Assert.AreEqual("Norman, Don", clippings[0].Author);
            Assert.AreEqual(ClippingType.Highlight, clippings[0].Type);
            Assert.AreEqual(145, clippings[0].PageNumber);
            Assert.AreEqual(3015, clippings[0].Location.Start);
            Assert.AreEqual(3016, clippings[0].Location.End);
            Assert.AreEqual(new DateTime(2019, 08, 15, 10, 14, 40), clippings[0].CreationDate);
            Assert.IsTrue(clippings[0].Content.StartsWith("Forcing functions can be"));

            Assert.AreEqual(ClippingType.Note, clippings[1].Type);
        }

        [TestMethod]
        public void ParseShouldExtractClippingsWithLocationWithoutPage()
        {
            const string sampleClippings =
                @"﻿The Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Highlight on Location 3026 | Added on Thursday, August 15, 2019 10:16:42 AM

Affordances refer to the potential actions that are possible, but these are easily discoverable only if they are perceivable: perceived affordances.
==========
Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Highlight on Location 3026-3027 | Added on Thursday, August 15, 2019 10:16:42 AM

Affordances refer to the potential actions that are possible, but these are easily discoverable only if they are perceivable: perceived affordances. It is the signifier component of the perceived affordance that allows people to determine the possible actions.
==========";

            this.WriteText(sampleClippings);

            var clippings = ClippingParser.Parse(this.stream).ToList();

            Assert.AreEqual(2, clippings.Count);
            Assert.AreEqual(-1, clippings[0].PageNumber);
            Assert.AreEqual(3026, clippings[0].Location.Start);
            Assert.AreEqual(3026, clippings[0].Location.End);

            Assert.AreEqual(-1, clippings[1].PageNumber);
            Assert.AreEqual(3026, clippings[1].Location.Start);
            Assert.AreEqual(3027, clippings[1].Location.End);
        }

        [TestMethod]
        public void ParseBookInfoShouldThrowIfBookInformationIsMalformed()
        {
            var line = "\ufeffSample Book";

            Assert.ThrowsException<InvalidClippingException>(() => this.clipping.ParseBookInfo(line));
        }

        [TestMethod]
        [DataRow("\ufeffSample Book ()", "Sample Book", "")]
        [DataRow("\ufeff ()", "", "")]
        public void ParseBookInfoShouldStoreBookNameAndAuthorInClipping(string line, string book, string author)
        {
            var clip = this.clipping.ParseBookInfo(line);

            Assert.IsNotNull(this.clipping);
            Assert.AreEqual(book, clip.Book);
            Assert.AreEqual(author, clip.Author);
        }

        [TestMethod]
        [DataRow("Sample annotation")]
        [DataRow("- Your ")]
        [DataRow("- Your NonHighlight on page 12 | Location 100-120 | Added on Thursday, August 15, 2019 10:17:58 AM")]
        [DataRow("- Your Highlight on page a | Location 100-120 | Added on Thursday, August 15, 2019 10:17:58 AM")]
        [DataRow("- Your Highlight on page 12 | Location a-120 | Added on Thursday, August 15, 2019 10:17:58 AM")]
        [DataRow("- Your Highlight on page 12 | Location 100-b | Added on Thursday, August 15, 2019 10:17:58 AM")]
        [DataRow("- Your Highlight on page 12 | Location 100-120 | Added on Thursday, August, 2019 10:17:58 AM")]
        public void ParseAnnotationInfoShouldThrowIfAnnotationInfoIsMissingOrInvalid(string line)
        {
            Assert.ThrowsException<InvalidClippingException>(
                () => this.clipping.ParseAnnotationInfo(line));
        }

        [TestMethod]
        [DataRow(ClippingType.Highlight, "- Your Highlight on page 12 | Location 100-120 | Added on Thursday, August 15, 2019 10:17:58 AM")]
        [DataRow(ClippingType.Note, "- Your Note on page 12 | Location 100-120 | Added on Thursday, August 15, 2019 10:17:58 AM")]
        public void ParseAnnotationInfoShouldCaptureAnnotationMetadata(ClippingType type, string line)
        {
            var clip = this.clipping.ParseAnnotationInfo(line);

            Assert.IsNotNull(clip);
            Assert.AreEqual(type, clip.Type);
            Assert.AreEqual(12, clip.PageNumber);
            Assert.AreEqual(new LineLocation(100, 120), clip.Location);
            Assert.AreEqual(new DateTime(2019, 08, 15, 10, 17, 58), clip.CreationDate);
        }

        [TestMethod]
        public void SkipBlankLineShouldThrowIfNonBlankLineIsProvided()
        {
            Assert.ThrowsException<InvalidClippingException>(() =>
                this.clipping.SkipBlankLine("xx"));
        }

        [TestMethod]
        public void SkipBlankLineShouldNotThrowForBlankLine()
        {
            var clip = this.clipping.SkipBlankLine(string.Empty);

            Assert.AreEqual(clip, this.clipping);
        }

        [TestMethod]
        [DataRow("x\n", "x\n" + ClippingParser.AnnotationEndMarker)]
        [DataRow("x\ny\n", "x\ny\n" + ClippingParser.AnnotationEndMarker)]
        public void ParseContentShouldExtractContentUntilAnnotationEndMarker(string content, string lines)
        {
            var clip = this.clipping.ParseContent(lines.Split('\n'));

            Assert.AreEqual(content.Replace("\n", Environment.NewLine), clip.Content);
        }

        public void Dispose()
        {
            this.writer?.Close();
        }

        private void WriteText(string text)
        {
            this.writer.Write(text);
            this.stream.Seek(0, SeekOrigin.Begin);
        }
    }
}