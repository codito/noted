// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Extensions.Readers;

    [TestClass]
    public class MobiReaderTests
    {
        [TestMethod]
        public void MobiReaderShouldParseAnnotationsAndTableOfContent()
        {
            using var stream = AssetFactory.GetAsset("pg42324.mobi");
            var reader = new MobiReader();

            var document = reader.Read(
                stream,
                new ReaderOptions(),
                docRef => new List<Annotation>());

            Assert.AreEqual(0, document.Annotations.Count());
        }
    }
}