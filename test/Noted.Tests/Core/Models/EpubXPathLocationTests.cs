// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Core.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noted.Core.Models;

[TestClass]
public class EpubXPathLocationTests
{
    [TestMethod]
    public void EpubXPathLocationShouldParseXPathLocation()
    {
        var pos0 = "/body/DocFragment[9]/body/article/p[3]/text().0";
        var pos1 = "/body/DocFragment[9]/body/article/p[3]/text().10";

        var loc = new EpubXPathLocation(pos0, pos1);

        Assert.IsNotNull(loc);
        Assert.AreEqual(9, loc.Start.DocumentFragmentId);
        Assert.AreEqual("/body/article/p[3]/text()", loc.Start.XPath);
        Assert.AreEqual(0, loc.Start.CharacterLocation);
    }
}