// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Extensions.Readers.Common
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Models;
    using Noted.Extensions.Readers.Common;

    [TestClass]
    public class HtmlContextParserTests
    {
        private const string SampleContent =
            @"<h1>Ch1</h1><p>Never call yourself a philosopher, nor talk a great deal among the unlearned about theorems, but act conformably to them.</p>
<h1>Ch2</h1>
<p></p>
<p>Thus, at an entertainment, don’t talk how persons ought to eat, but eat as you ought.</p>
<blockquote><div></div></blockquote>
<p>And, if anyone tells you that you know nothing, and you are not nettled at it, then you may be sure that you have begun your business.</p>";

        private static readonly List<DocumentSection> SampleSections = new()
        {
            new() { Title = "Ch1", Level = 1, Location = 0 },
            new() { Title = "Ch2", Level = 1, Location = 141 }
        };

        private readonly List<Annotation> annotations;
        private readonly HtmlContextParser parser;

        public HtmlContextParserTests()
        {
            this.parser = new HtmlContextParser();
            this.annotations = new List<Annotation>
            {
                new() { Content = "call yourself a philosopher", Context = new AnnotationContext() },
                new() { Content = "eat as you ought. And, if anyone", Context = new AnnotationContext() }
            };
        }

        [TestMethod]
        public async Task HtmlContextParserShouldAddContextToAnnotations()
        {
            var (a, _, _) = await this.parser.AddContext(
                SampleContent,
                SampleSections,
                new[] { (new LineLocation(1, 2), this.annotations[0]) });

            Assert.AreEqual(1, a.Count);
            Assert.IsTrue(a[0].Context.Content.StartsWith("Never"));
            Assert.AreEqual("Ch1", a[0].Context.DocumentSection.Title);
        }

        [TestMethod]
        public async Task HtmlContextParserShouldAddContextWhenAnnotationSpansOverElements()
        {
            var (a, _, _) = await this.parser.AddContext(
                SampleContent,
                SampleSections,
                new[] { (new LineLocation(1, 2), this.annotations[1]) });

            Assert.AreEqual(1, a.Count);
            Assert.IsTrue(a[0].Context.Content.StartsWith("Thus, "));
            Assert.AreEqual("Ch2", a[0].Context.DocumentSection.Title);
        }
    }
}