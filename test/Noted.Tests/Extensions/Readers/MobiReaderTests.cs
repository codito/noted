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
    using Noted.Extensions.Readers;

    [TestClass]
    public class MobiReaderTests
    {
        [TestMethod]
        public void MobiReaderShouldSupportMobiFileExtensions()
        {
            var reader = new MobiReader();

            CollectionAssert.AreEquivalent(new[] { "mobi" }, reader.SupportedExtensions);
        }

        [TestMethod]
        public async Task MobiReaderShouldParseAnnotationsAndTableOfContent()
        {
            await using var stream = AssetFactory.GetAsset("pg42324.mobi");
            var reader = new MobiReader();
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

            var document = await reader.Read(
                stream,
                new ReaderOptions(),
                _ => annotations);

            Assert.AreEqual(1, document.Annotations.Count());
        }
    }
}