// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Writers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noted.Core;
using Noted.Core.Models;
using Noted.Core.Platform.IO;
using Noted.Extensions.Writers;

[TestClass]
public class MarkdownWriterTests
{
    [TestMethod]
    public async Task MarkdownWriterShouldNotCrashForDocumentWithoutSections()
    {
        var config = new Configuration { ExtractDocumentSections = true };
        var document = new Document();
        var stream = new MemoryStream();
        var writer = new MarkdownWriter(new NullLogger());

        document.Annotations = new List<Annotation>
        {
            new(
                "dummy annotation",
                document,
                AnnotationType.Highlight,
                new AnnotationContext(),
                DateTime.Now)
        };

        await writer.Write(config, document, stream);
    }
}