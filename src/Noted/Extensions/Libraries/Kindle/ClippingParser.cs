// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Noted.Platform.IO;

    public static class ClippingParser
    {
        public const string AnnotationEndMarker = "==========";

        private static readonly Regex BookInfoRegex = new(
            @"^\ufeff?(?<bookname>.*)\s\((?<bookauthor>.*)\)$",
            RegexOptions.Compiled);

        private static readonly Regex AnnotationInfoRegex = new(
            @"- Your (?<annotationType>\w+) on page (?<pageNumber>\d+) \| Location (?<startLoc>\d+)-?(?<endLoc>\d+)? \| Added on (?<date>.*)$",
            RegexOptions.Compiled);

        private static readonly Regex AnnotationInfoRegexWithoutPage = new(
            @"- Your (?<annotationType>\w+) on Location (?<startLoc>\d+)-?(?<endLoc>\d+)? \| Added on (?<date>.*)$",
            RegexOptions.Compiled);

        public static IEnumerable<Clipping> Parse(Stream stream)
        {
            // Annotations format
            //  <Book name> (Book author)
            //  - Your (Highlight|Note) on page <number> | Location x-y | Added on <date>
            //
            //  <content>
            //  ==========
            var reader = new StreamReader(stream, Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // We have an annotation block. Start processing!
                yield return new Clipping()
                    .ParseBookInfo(line)
                    .ParseAnnotationInfo(reader.ReadLine())
                    .SkipBlankLine(reader.ReadLine())
                    .ParseContent(reader.AsEnumerable());
            }
        }

        public static Clipping ParseBookInfo(this Clipping clipping, string line)
        {
            var match = BookInfoRegex.Match(line);
            if (!match.Success)
            {
                throw new InvalidClippingException(
                    "Book information line doesn't match regex", line);
            }

            clipping.Book = match.Groups["bookname"].Value;
            clipping.Author = match.Groups["bookauthor"].Value;

            return clipping;
        }

        public static Clipping ParseAnnotationInfo(
            this Clipping clipping,
            string line)
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("- "))
            {
                throw new InvalidClippingException(
                    "Annotation information is null or invalid", line);
            }

            var match = AnnotationInfoRegex.Match(line);
            if (!match.Success)
            {
                // Attempt to parse annotation info without page number
                match = AnnotationInfoRegexWithoutPage.Match(line);
                if (!match.Success)
                {
                    throw new InvalidClippingException(
                        "Annotation information is missing fields", line);
                }
            }

            try
            {
                clipping.Type = Enum.Parse<ClippingType>(match.Groups["annotationType"].Value);
                clipping.CreationDate = DateTime.ParseExact(match.Groups["date"].Value, "F", CultureInfo.CreateSpecificCulture("en-US"));

                var startIndex = new Index(int.Parse(match.Groups["startLoc"].Value));
                var endIndex = startIndex;
                if (match.Groups["endLoc"].Success)
                {
                    endIndex = new(int.Parse(match.Groups["endLoc"].Value));
                }

                clipping.Location = new Range(startIndex, endIndex);
                if (match.Groups["pageNumber"].Success)
                {
                    clipping.PageNumber = int.Parse(match.Groups["pageNumber"].Value);
                }
            }
            catch (Exception e)
            {
                throw new InvalidClippingException(
                    "Error parsing annotation information fields", line, e);
            }

            return clipping;
        }

        public static Clipping SkipBlankLine(this Clipping clipping, string line)
        {
            if (line != null && line.Length == 0)
            {
                return clipping;
            }

            throw new InvalidClippingException("Expected blank line", line);
        }

        public static Clipping ParseContent(this Clipping clipping, IEnumerable<string> lines)
        {
            var text = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.Equals(AnnotationEndMarker, StringComparison.InvariantCulture))
                {
                    break;
                }

                text.AppendLine(line);
            }

            clipping.Content = text.ToString();
            return clipping;
        }
    }
}