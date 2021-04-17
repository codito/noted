// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using Noted.Extensions.Readers;

    [TestClass]
    public class MobiReaderTests
    {
        private readonly MobiReader reader;

        public MobiReaderTests()
        {
            this.reader = new MobiReader(new NullLogger());
        }

        [TestMethod]
        public void MobiReaderShouldSupportMobiFileExtensions()
        {
            CollectionAssert.AreEquivalent(new[] { "mobi" }, this.reader.SupportedExtensions);
        }

        [TestMethod]
        public async Task MobiReaderShouldParseAnnotationsAndTableOfContent()
        {
            await using var stream = AssetFactory.GetAsset("pg42324.mobi");
            var annotations = new List<Annotation>
            {
                new(
                    "Nothing is so painful to the human mind as a great and sudden change.",
                    new DocumentReference(),
                    AnnotationType.Highlight,
                    new AnnotationContext
                    {
                        PageNumber = 380,
                        SerializedLocation = "line://925-994"
                    },
                    DateTime.MinValue)
            };

            var document = await this.reader.Read(
                stream,
                new ReaderOptions(),
                _ => annotations);

            Assert.AreEqual(1, document.Annotations.Count());
        }
    }
}