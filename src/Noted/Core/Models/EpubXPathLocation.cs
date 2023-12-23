// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models;

using System;
using System.Text.RegularExpressions;

public readonly struct EpubXPathLocation(string pos0, string pos1) : IComparable
{
    public EpubLocation Start { get; init; } = EpubLocation.FromString(pos0);

    public EpubLocation End { get; init; } = EpubLocation.FromString(pos1);

    public static EpubXPathLocation FromString(string location)
    {
        var range = new Uri(location).PathAndQuery.Split('-');
        return new EpubXPathLocation(range[0], range[1]);
    }

    public override readonly string ToString() => $"epubxpath://{this.Start}-{this.End}";

    public readonly int CompareTo(object? obj)
    {
        if (obj is not EpubXPathLocation other)
        {
            throw new ArgumentException(null, nameof(obj));
        }

        var startCompare = this.Start.CompareTo(other.Start);
        return startCompare == 0 ? this.End.CompareTo(other.End) : startCompare;
    }
}

public partial record EpubLocation(
    int DocumentFragmentId,
    string XPath,
    int CharacterLocation) : IComparable
{
    public int CompareTo(object? obj)
    {
        if (obj is not EpubLocation other)
        {
            throw new ArgumentException(null, nameof(obj));
        }

        var docFragmentCompare = this.DocumentFragmentId.CompareTo(other.DocumentFragmentId);
        if (docFragmentCompare != 0)
        {
            return docFragmentCompare;
        }

        // Comparing xpaths is impossible :( We'll take a chance to compare lexically, assuming the structure of book pages to be consistent.
        // TODO: It is better to probably keep the original order of elements.
        var xpathCompare = this.XPath.CompareTo(other.XPath);
        if (xpathCompare != 0)
        {
            return xpathCompare;
        }

        return this.CharacterLocation.CompareTo(other.CharacterLocation);
    }

    public override string ToString() => $"/body/DocFragment[{this.DocumentFragmentId}]{this.XPath}.{this.CharacterLocation}";

    public static EpubLocation FromString(string xpath)
    {
        var match = EpubXPathRegex().Match(xpath);
        if (!match.Success ||
         !int.TryParse(match.Groups["docFragmentId"].Value, out var docFragmentId) ||
         string.IsNullOrEmpty(match.Groups["xpath"].Value) ||
         !int.TryParse(match.Groups["charIndex"].Value, out var charIndex))
        {
            throw new ArgumentException(
                $"Invalid xpath: {xpath}", nameof(xpath));
        }

        return new(docFragmentId, match.Groups["xpath"].Value, charIndex);
    }

    [GeneratedRegex(@"/body/DocFragment\[(?<docFragmentId>\d+)\](?<xpath>.*)\.(?<charIndex>.*)$", RegexOptions.Compiled)]
    private static partial Regex EpubXPathRegex();
}
