// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using Noted.Extensions.Readers;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Fonts.Standard14Fonts;
    using UglyToad.PdfPig.Writer;

    [TestClass]
    public class PdfReaderTests
    {
        // For manual tests, try these papers:
        // - Crash-only software
        // - Mundanity of excellence by Chambliss et al
        private const string SampleText =
            "Now, the things within our power are by nature free, unrestricted, unhindered; but those beyond our power are weak, dependent, restricted, alien. Remember, then, that if you attribute freedom to things by nature dependent, and take what belongs to others for your own, you will be hindered, you will lament, you will be disturbed, you will find fault both with gods and men. But if you take for your own only that which is your own, and view what belongs to others just as it really is, then no one will ever compel you, no one will restrict you, you will find fault with no one, you will accuse no one, you will do nothing against your will; no one will hurt you, you will not have an enemy, nor will you suffer any harm.";

        private static readonly string[] EXPECTED = ["pdf"];
        private readonly PdfReader reader;
        private readonly List<Annotation> emptyExternalAnnotations = [];

        public PdfReaderTests()
        {
            this.reader = new PdfReader(new NullLogger());
        }

        [TestMethod]
        public void PdfReaderShouldSupportOnlyPdfFileExtension()
        {
            CollectionAssert.AreEqual(EXPECTED, this.reader.SupportedExtensions);
        }

        [TestMethod]
        public void PdfReaderShouldThrowIfStreamIsInvalid()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                this.reader.Read(null, new ReaderOptions(), this.emptyExternalAnnotations));
        }

        [TestMethod]
        public void PdfReaderShouldThrowIfStreamIsUnreadable()
        {
            using var stream = new MemoryStream();
            stream.Close();

            Assert.ThrowsException<ArgumentException>(() =>
                this.reader.Read(stream, new ReaderOptions(), this.emptyExternalAnnotations));
        }

        [TestMethod]
        public async Task ReadShouldParseDocumentMetadata()
        {
            await using var stream = new MemoryStream();
            stream.Write(CreatePdfWithMetadata());
            stream.Seek(0, SeekOrigin.Begin);

            var document = await this.reader.Read(stream, new ReaderOptions(), this.emptyExternalAnnotations);

            Assert.IsNotNull(document);
            Assert.AreEqual("Sample title", document.Title);
            Assert.AreEqual("John Doe", document.Author);
            Assert.AreEqual("Philosophy", document.Subject);
            Assert.AreEqual("Philosophy; Eastern;", document.Keywords);
            Assert.IsFalse(document.Annotations.Any());
        }

        [TestMethod]
        public async Task ReadShouldParseSingleColumnLayoutDocument()
        {
            await using var fs = AssetFactory.GetAsset("pdf", "single column.pdf");

            var document = await this.reader.Read(fs, new ReaderOptions(), this.emptyExternalAnnotations);

            var annotations = document.Annotations.ToList();
            Assert.AreEqual(2, annotations.Count);
            Assert.AreEqual(
                "Nam id ante vitae erat consequat bibendum quis vitae sapien. Etiam ac quam ac felis gravida rutrum. ",
                annotations[0].Content);
            Assert.AreEqual(
                "Fusce eu sem lacus. Donec euismod eleifend dolor, a tristique magna iaculis eu. Proin elit erat, imperdiet vel condimentum eu, placerat non augue. Nam id ante vitae erat consequat bibendum quis vitae sapien. Etiam ac quam ac felis gravida rutrum. Quisque quis egestas risus. Nam interdum nisi sit amet augue tincidunt, dapibus commodo ipsum malesuada.",
                annotations[0].Context.Content);
            Assert.AreEqual(AnnotationType.Highlight, annotations[0].Type);
        }

        [TestMethod]
        public async Task ReadShouldParseTwoColumnLayoutDocument()
        {
            await using var fs = AssetFactory.GetAsset("pdf", "two column.pdf");

            var document = await this.reader.Read(fs, new ReaderOptions(), this.emptyExternalAnnotations);

            var annotations = document.Annotations.ToList();
            Assert.AreEqual(5, annotations.Count);

            // Multiple paragraphs are joined in context because there isn't much
            // space between paragraph formatting
            Assert.AreEqual(
                "Wellbeing pleiades mind, wavelength intelligence formation mind well-being constellation experience. Mass matrix mass mercury spiral galactic, existence wavelength galactic spirit geometry century wavelength. Wellbeing galaxies objects happiness emotional vibration, planets physics wavelength modern goddess star spiral. Century relations planets galaxies, spiritual university spiritual university heal magic flow intelligence. Core mass wave existence geometry brain modern. University constellation philosophical solar star star materialist spiral earth, spiral meaning mystical existence. Hubble sirius human solar spirituality happiness, emotional pleiades spiritual relativity earth. Mind objects formation goddess frequency matrix modern. Physics consciousness mystical geometry star natural galaxies. Relations happiness spiral sirius star atoms flow. Mercury time emotional energy century, constellation relations gaia experience spectrum intelligence core frequency frequency. Gaia mystical philosophical happiness new wellbeing materialist galaxies human.",
                annotations[0].Context.Content);
            Assert.AreEqual(
                "gaia experience spectrum intelligence ",
                annotations[0].Content);
            Assert.AreEqual(AnnotationType.Highlight, annotations[0].Type);
        }

        private static ReadOnlySpan<byte> CreatePdfWithMetadata()
        {
            var builder = new PdfDocumentBuilder();
            var page = builder.AddPage(PageSize.A4);
            var font = builder.AddStandard14Font(Standard14Font.Helvetica);

            builder.DocumentInformation.Title = "Sample title";
            builder.DocumentInformation.Author = "John Doe";
            builder.DocumentInformation.Subject = "Philosophy";
            builder.DocumentInformation.Keywords = "Philosophy; Eastern;";
            builder.IncludeDocumentInformation = true;

            page.AddText(SampleText, 12, new PdfPoint(25, 700), font);

            return builder.Build();
        }
    }
}