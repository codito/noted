// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Libraries.Kindle
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Models;
    using Noted.Extensions.Libraries.Kindle;

    [TestClass]
    public class ClippingExtensionsTests
    {
        [TestMethod]
        [DataRow(ClippingType.Highlight, AnnotationType.Highlight)]
        [DataRow(ClippingType.Note, AnnotationType.Note)]
        public void ToAnnotationShouldConvertClippingToAnnotation(ClippingType type, AnnotationType expectedType)
        {
            var clipping = new Clipping
            {
                Author = "Dummy author",
                Book = "Dummy book",
                Content = "Sample highlighted content",
                CreationDate = new DateTime(2019, 08, 15, 10, 48, 48),
                PageNumber = 90,
                Location = new Range(new(10), new(12)),
                Type = type
            };

            var annotation = clipping.ToAnnotation();

            Assert.IsNotNull(annotation);
            Assert.AreEqual(clipping.Content, annotation.Content);
            Assert.AreEqual(clipping.CreationDate, annotation.CreatedDate);
            Assert.AreEqual(clipping.PageNumber, annotation.Context.PageNumber);
            Assert.AreEqual("line://10-12", annotation.Context.SerializedLocation);
            Assert.AreEqual(clipping.Book, annotation.Document.Title);
            Assert.AreEqual(clipping.Author, annotation.Document.Author);
            Assert.AreEqual(expectedType, annotation.Type);
        }
    }
}