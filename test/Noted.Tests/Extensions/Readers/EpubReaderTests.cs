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
                SerializedLocation = @"{""Start"":{""DocumentFragmentId"":9,""XPath"":""/body/DocFragment[9]/body/article/p[3]/text()"",""CharacterLocation"":0},""End"":{""DocumentFragmentId"":9,""XPath"":""/body/DocFragment[9]/body/article/p[3]/text()"", ""CharacterLocation"":20},""PageNumber"":21,""SequenceOrder"":22}"
            },
            new DateTime(2023, 12, 23));

        var document = await new EpubReader(this.logger).Read(stream, new Noted.Core.Extensions.ReaderOptions(), [annotation]);

        var annotations = document.Annotations.ToList();
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual("On Giving", annotations[0].Context.DocumentSection.Title);
        Assert.AreNotEqual(0, annotations[0].Context.DocumentSection.Location);
        Assert.AreEqual(0, annotations[0].Context.Location);
        Assert.AreEqual(0, annotations[0].Context.PageNumber);
        Assert.AreEqual(34, document.Sections.Count());
        Assert.AreEqual(new DateTime(2023, 12, 23), document.CreatedDate);
        Assert.AreEqual(new DateTime(2023, 12, 23), document.ModifiedDate);
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
                SerializedLocation = @"{""Start"":{""DocumentFragmentId"":7,""XPath"":""/body/DocFragment[7]/body/section/section[17]/p/text()[2]"",""CharacterLocation"":2941},""End"":{""DocumentFragmentId"":7,""XPath"":""/body/DocFragment[7]/body/section/section[17]/p/text()[2]"",""CharacterLocation"":3124},""PageNumber"":1,""SequenceNumber"":12}"
            },
            new DateTime(2023, 12, 23));

        var document = await new EpubReader(this.logger).Read(stream, new Noted.Core.Extensions.ReaderOptions(), [annotation]);

        var annotations = document.Annotations.ToList();
        Assert.AreEqual(1, annotations.Count);
        Assert.AreEqual("XVII", annotations[0].Context.DocumentSection.Title);
        Assert.AreNotEqual(0, annotations[0].Context.DocumentSection.Location);
        Assert.AreEqual(0, annotations[0].Context.Location); // Annotation Location is not updated by EpubReader
        Assert.AreEqual(552, document.Sections.Count());
        Assert.AreEqual(new DateTime(2023, 12, 23), document.CreatedDate);
        Assert.AreEqual(new DateTime(2023, 12, 23), document.ModifiedDate);
    }

    [TestMethod]
    public async Task EpubReaderShouldParseFirstAndLastAnnotationDates()
    {
        using var stream = AssetFactory.GetAsset(AssetFactory.GetKOReaderLibrary(), "dialogues_seneca.epub");
        var annotation1 = new Annotation(
            "Annotation 1",
            new DocumentReference { Title = "Seneca’s dialogues and consolations, including “On Benefits,” examine living life through the lens of Stoic philosophy." },
            AnnotationType.Highlight,
            new AnnotationContext
            {
                DocumentSection = new DocumentSection("XVII", 0, 0, null),
                SerializedLocation = @"{""Start"":{""DocumentFragmentId"":7,""XPath"":""/body/DocFragment[7]/body/section/section[17]/p/text()[2]"",""CharacterLocation"":2941},""End"":{""DocumentFragmentId"":7,""XPath"":""/body/DocFragment[7]/body/section/section[17]/p/text()[2]"",""CharacterLocation"":3124},""PageNumber"":1,""SequenceNumber"":12}"
            },
            new DateTime(2023, 12, 23));
        var annotation2 = new Annotation(
            "Annotation 2",
            new DocumentReference { Title = "Seneca’s dialogues and consolations, including “On Benefits,” examine living life through the lens of Stoic philosophy." },
            AnnotationType.Highlight,
            new AnnotationContext
            {
                DocumentSection = new DocumentSection("XVII", 0, 0, null),
                SerializedLocation = @"{""Start"":{""DocumentFragmentId"":7,""XPath"":""/body/DocFragment[7]/body/section/section[17]/p/text()[2]"",""CharacterLocation"":2941},""End"":{""DocumentFragmentId"":7,""XPath"":""/body/DocFragment[7]/body/section/section[17]/p/text()[2]"",""CharacterLocation"":3124},""PageNumber"":1,""SequenceNumber"":12}"
            },
            new DateTime(2023, 12, 24));

        var document = await new EpubReader(this.logger).Read(stream, new Noted.Core.Extensions.ReaderOptions(), [annotation1, annotation2]);

        Assert.AreEqual(new DateTime(2023, 12, 23), document.CreatedDate);
        Assert.AreEqual(new DateTime(2023, 12, 24), document.ModifiedDate);
    }
}