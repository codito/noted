// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Libraries
{
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Extensions.Libraries.KOReader;

    [TestClass]
    public class KOReaderAnnotationProviderTests
    {
        [TestMethod]
        public void KOReaderAnnotationProviderIsNotAvailable()
        {
            var kap = new KOReaderAnnotationProvider();

            Assert.IsFalse(kap.IsAvailable(Path.GetTempFileName()));
        }

        [TestMethod]
        public void KOReaderAnnotationProviderShouldReturnZeroAnnotations()
        {
            var kap = new KOReaderAnnotationProvider();

            var annotations =
                kap.GetAnnotations(Path.GetTempFileName()).ToList();

            Assert.AreEqual(0, annotations.Count);
        }
    }
}