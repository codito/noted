// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Acceptance;

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class AcceptanceTests
{
    [TestMethod]
    public async Task KindleLibraryShouldExtractAnnotationsWithDocumentSections()
    {
        var outputPath = Path.Join(AssetFactory.TestDataDir, "kindle");
        AssetFactory.EnsureDirectory(outputPath);

        var result = await Program.Main([AssetFactory.GetKindleLibrary(), outputPath]);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task KOReaderLibraryShouldExtractAnnotationsWithDocumentSections()
    {
        var outputPath = Path.Join(AssetFactory.TestDataDir, "koreader");
        AssetFactory.EnsureDirectory(outputPath);

        var result = await Program.Main([AssetFactory.GetKOReaderLibrary(), outputPath]);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task PdfLibraryShouldExtractAnnotations()
    {
        var outputPath = Path.Join(AssetFactory.TestDataDir, "pdf");
        AssetFactory.EnsureDirectory(outputPath);

        var result = await Program.Main([AssetFactory.GetPdfLibrary(), outputPath]);

        Assert.AreEqual(0, result);
    }
}