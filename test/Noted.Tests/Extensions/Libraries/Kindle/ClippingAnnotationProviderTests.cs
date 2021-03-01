// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Libraries.Kindle
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Noted.Core.Platform.IO;
    using Noted.Extensions.Libraries.Kindle;

    [TestClass]
    public class ClippingAnnotationProviderTests
    {
        private const string SampleClippings = @"﻿The Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Highlight on page 145 | Location 3015-3016 | Added on Thursday, August 15, 2019 10:14:40 AM

Forcing functions can be a nuisance in normal usage. The result is that many people will deliberately disable the forcing function, thereby negating its safety feature.
==========
﻿The Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Highlight on page 145 | Location 3026-3027 | Added on Thursday, August 15, 2019 10:16:42 AM

Affordances refer to the potential actions that are possible, but these are easily discoverable only if they are perceivable: perceived affordances. It is the signifier component of the perceived affordance that allows people to determine the possible actions.
==========
﻿The Design of Everyday Things: Revised and Expanded Edition (Norman, Don)
- Your Highlight on page 145 | Location 3032-3033 | Added on Thursday, August 15, 2019 10:17:58 AM

The interpretation of a perceived affordance is a cultural convention.
==========
";

        [TestMethod]
        public void GetClippingsShouldRetrieveDocumentAnnotations()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Unicode);
            writer.Write(SampleClippings);
            stream.Seek(0, SeekOrigin.Begin);

            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(f => f.IsDirectory(It.IsAny<string>()))
                .Returns(true);
            fileSystem.Setup(f => f.Exists(It.IsAny<string>()))
                .Returns(true);
            fileSystem.Setup(f => f.OpenPathForRead(It.IsAny<string>()))
                .Returns(stream);
            var kindle = new ClippingAnnotationProvider(fileSystem.Object);
            var clippings = kindle.GetAnnotations("dummyPath").ToList();

            Assert.AreEqual(3, clippings.Count);
        }
    }
}