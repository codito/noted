// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Libraries
{
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Noted.Core.Platform.IO;
    using Noted.Extensions.Libraries.KOReader;

    [TestClass]
    public class KOReaderAnnotationProviderTests
    {
        private static readonly string DummyTempFile = Path.Join(Path.GetTempPath(), "pg42324.sdr", "metadata.epub.lua");
        private readonly Mock<ILogger> logger;
        private readonly Mock<IFileSystem> fileSystem;
        private readonly KOReaderAnnotationProvider annotationProvider;

        public KOReaderAnnotationProviderTests()
        {
            this.logger = new Mock<ILogger>();
            this.fileSystem = new Mock<IFileSystem>();
            this.fileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([DummyTempFile]);

            this.annotationProvider = new KOReaderAnnotationProvider(this.fileSystem.Object, this.logger.Object);
        }

        [TestMethod]
        public void KOReaderAnnotationProviderIsAvailable()
        {
            Assert.IsTrue(this.annotationProvider.IsAvailable(Path.GetTempPath()));
        }

        [TestMethod]
        public void KOReaderAnnotationProviderIsNotAvailable()
        {
            this.fileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([]);

            Assert.IsFalse(this.annotationProvider.IsAvailable(Path.GetTempPath()));
        }

        [TestMethod]
        public void KOReaderAnnotationProviderShouldReturnZeroAnnotations()
        {
            this.fileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([]);

            var annotations =
                this.annotationProvider.GetAnnotations(AssetFactory.GetKindleLibrary()).ToList();

            Assert.AreEqual(0, annotations.Count);
        }
    }
}