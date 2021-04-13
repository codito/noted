// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers.Navigation
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Extensions.Readers.Navigation;

    [TestClass]
    public class HtmlTableOfContentParserTests
    {
        private const string TocFragment = @"
<p height=""1em"" width=""0pt"" align=""center"">
  <font size=""5"">
    <b>Table of Contents</b>
  </font>
</p>
<p height=""1em"" width=""-14pt"">
  <a filepos=0000003859>Preface</a>
</p>
<blockquote height=""0pt"">
  <blockquote width=""0pt"">
    <a filepos=0000003969>Section 0.1</a>
  </blockquote>
</blockquote>
<blockquote height=""0pt"">
  <blockquote width=""0pt"">
    <a filepos=0000004860>Section 0.2</a>
  </blockquote>
</blockquote>
<p height=""0pt"" width=""-14pt"">
  <a filepos=0000019360>1. Introduction</a>
</p>
<blockquote height=""0pt"">
  <blockquote width=""0pt"">
    <a filepos=0000021076>Section 1.1</a>
  </blockquote>
</blockquote>
<p height=""0pt"" width=""-14pt"">
  <a filepos=0000043219>2. Chapter two</a>
</p>
<p height=""0pt"" width=""-14pt"">
  <a filepos=0000072853>3. Chapter three</a>
</p>
<p height=""0pt"" width=""-14pt"">
  <a filepos=0000347294>4. Conclusion</a>
</p>
<p height=""0pt"" width=""-14pt"">
  <a filepos=0000351014>Index</a>
</p>
<mbp:pagebreak/>
";

        private readonly HtmlTableOfContentParser parser;

        public HtmlTableOfContentParserTests()
        {
            this.parser = new HtmlTableOfContentParser();
        }

        [TestMethod]
        public async Task ParseShouldReturnTableOfContentWithDepth()
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TocFragment));
            var toc = await this.parser.Parse(stream).ToListAsync();

            Assert.AreEqual(9, toc.Count);
            Assert.AreEqual("Preface", toc[0].Title);
            Assert.AreEqual(1, toc[0].Level);
            Assert.AreEqual(3859, toc[0].Location);
            Assert.AreEqual("Section 1.1", toc[4].Title);
            Assert.AreEqual(2, toc[4].Level);
            Assert.AreEqual(21076, toc[4].Location);
        }
    }
}