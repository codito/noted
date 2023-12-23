// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Noted.Core.Models;
using Noted.Core.Platform.IO;
using Noted.Extensions.Readers;

[TestClass]
public class EpubReaderTests
{
    private readonly ILogger logger;

    public EpubReaderTests()
    {
        this.logger = new Mock<ILogger>().Object;
    }

    [TestMethod]
    public async Task EpubReaderShouldParseDocumentWithSections()
    {
        using var stream = AssetFactory.GetAsset(AssetFactory.GetKOReaderLibrary(), "the_prophet.epub");

        var document = await new EpubReader(this.logger).Read(stream, new Noted.Core.Extensions.ReaderOptions(), []);

        Assert.AreEqual(0, document.Annotations.Count());
        Assert.AreEqual(0, document.Sections.Count());
        Assert.AreEqual("The Prophet", document.Title);
        Assert.AreEqual("Khalil Gibran", document.Author);
    }

    [TestMethod]
    public async Task EpubReaderShouldParseDocumentWithAnnotations()
    {
        using var stream = AssetFactory.GetAsset(AssetFactory.GetKOReaderLibrary(), "the_prophet.epub");
        var annotation = new Annotation(
            "Test highlight",
            new DocumentReference { Title = "The Prophet" },
            AnnotationType.Highlight,
            new AnnotationContext
            {
                DocumentSection = new DocumentSection("On Giving", 0, 0, null),
                SerializedLocation = "epubxpath:///body/DocFragment[9]/body/article/p[3]/text().0-/body/DocFragment[9]/body/article/p[3]/text().20"
            },
            new DateTime(2023, 12, 23));

        var document = await new EpubReader(this.logger).Read(stream, new Noted.Core.Extensions.ReaderOptions(), [annotation]);

        var annotations = document.Annotations.ToList();
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual("On Giving", annotations[0].Context.DocumentSection.Title);
        Assert.AreNotEqual(0, annotations[0].Context.DocumentSection.Location);
        Assert.AreNotEqual(0, annotations[0].Context.Location);
        Assert.AreEqual(34, document.Sections.Count());
    }

    [TestMethod]
    public async Task EpubReaderShouldParseEpub3DocumentWithAnnotations()
    {
        using var stream = AssetFactory.GetAsset(AssetFactory.GetKOReaderLibrary(), "dialogues_seneca.epub");
        var annotation = new Annotation(
            "We shall never lack causes of anxiety, either pleasurable or painful: our life will be pushed along from one business to another: leisure will always be wished for, and never enjoyed.",
            new DocumentReference { Title = "Seneca’s dialogues and consolations, including “On Benefits,” examine living life through the lens of Stoic philosophy." },
            AnnotationType.Highlight,
            new AnnotationContext
            {
                DocumentSection = new DocumentSection("XVII", 0, 0, null),
                SerializedLocation = "epubxpath:///body/DocFragment[7]/body/section/section[17]/p/text()[2].2941-/body/DocFragment[7]/body/section/section[17]/p/text()[2].3124"
            },
            new DateTime(2023, 12, 23));

        var document = await new EpubReader(this.logger).Read(stream, new Noted.Core.Extensions.ReaderOptions(), [annotation]);

        var annotations = document.Annotations.ToList();
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual("XVII", annotations[0].Context.DocumentSection.Title);
        Assert.AreNotEqual(0, annotations[0].Context.DocumentSection.Location);
        Assert.AreNotEqual(0, annotations[0].Context.Location);
        Assert.AreEqual(552, document.Sections.Count());
    }
}