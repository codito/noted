// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Core.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiscUtil.Collections.Extensions;
using Noted.Core.Models;

[TestClass]
public class EpubXPathLocationTests
{
    [TestMethod]
    public void EpubXPathLocationShouldParseXPathLocation()
    {
        var pos0 = "/body/DocFragment[9]/body/article/p[3]/text().0";
        var pos1 = "/body/DocFragment[9]/body/article/p[3]/text().10";

        var loc = new EpubXPathLocation(pos0, pos1, 0, 0);

        Assert.IsNotNull(loc);
        Assert.AreEqual(9, loc.Start.DocumentFragmentId);
        Assert.AreEqual("/body/article/p[3]/text()", loc.Start.XPath);
        Assert.AreEqual(0, loc.Start.CharacterLocation);
    }

    [TestMethod]
    public void EpubXPathLocationShouldSerializeLocationToJsonFormat()
    {
        var pos0 = "/body/DocFragment[9]/body/article/p[3]/text().0";
        var pos1 = "/body/DocFragment[9]/body/article/p[3]/text().10";
        var loc = new EpubXPathLocation(pos0, pos1, 0, 0);

        var serializedLocation = loc.ToString();

        Assert.AreEqual(@"{""Start"":{""DocumentFragmentId"":9,""XPath"":""/body/article/p[3]/text()"",""CharacterLocation"":0},""End"":{""DocumentFragmentId"":9,""XPath"":""/body/article/p[3]/text()"",""CharacterLocation"":10},""PageNumber"":0,""SequenceNumber"":0}", serializedLocation);
    }

    [TestMethod]
    public void EpubXPathLocationShouldDeserializeLocationFromJsonFormat()
    {
        var jsonString = @"{""Start"":{""DocumentFragmentId"":9,""XPath"":""/body/article/p[3]/text()"",""CharacterLocation"":0},""End"":{""DocumentFragmentId"":9,""XPath"":""/body/article/p[3]/text()"",""CharacterLocation"":10},""PageNumber"":7,""SequenceNumber"":11}";

        var loc = EpubXPathLocation.FromString(jsonString);

        Assert.AreEqual(9, loc.Start.DocumentFragmentId);
        Assert.AreEqual("/body/article/p[3]/text()", loc.Start.XPath);
        Assert.AreEqual(10, loc.End.CharacterLocation);
        Assert.AreEqual(7, loc.PageNumber);
        Assert.AreEqual(11, loc.SequenceNumber);
    }
}