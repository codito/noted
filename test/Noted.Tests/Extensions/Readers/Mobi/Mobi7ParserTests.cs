// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers.Mobi
{
    using System.Threading.Tasks;
    using Ephemerality.Unpack.Mobi;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Extensions.Readers.Mobi;

    [TestClass]
    public class Mobi7ParserTests
    {
        [TestMethod]
        public async Task Mobi7ParserShouldExtractNavigationStream()
        {
            await using var stream = AssetFactory.GetAsset("pg42324.mobi");
            await using var rawMlStream = new MobiMetadata(stream).GetRawMlStream();

            var parser = new Mobi7Parser();
            var contentStream = await Mobi7Parser.GetNavigationStream(rawMlStream);

            Assert.IsNotNull(contentStream);
            Assert.AreEqual(4133, contentStream.Length);
        }
    }
}