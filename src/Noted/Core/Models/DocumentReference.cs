// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DocumentReference
    {
        private SortedSet<string> authorVector;

        public string Title { get; init; }

        public string Author { get; init; }

        private SortedSet<string> AuthorVector
        {
            get
            {
                if (this.authorVector != null)
                {
                    return this.authorVector;
                }

                this.authorVector = new SortedSet<string>(this.Author
                    .Split(' ')
                    .Select(x => x.Trim(' ', ',', '.').ToLower()));
                return this.authorVector;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is DocumentReference right))
            {
                return false;
            }

            return this.Title.Equals(right.Title, StringComparison.Ordinal) &&
                   this.Author.Equals(right.Author, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return $"{this.Title}{this.Author}".GetHashCode();
        }

        // TODO Add support for matching Asin or ISBN or DOI
        public bool IsSimilar(DocumentReference other)
        {
            // Attempt memberwise equality comparison
            if (this.Equals(other))
            {
                return true;
            }

            // Use stringent matching for titles since mostly the titles may
            // include subtitles, thus one will be substring of another
            var titleMatch = this.Title.Length < other.Title.Length
                ? other.Title.ToLower().Contains(this.Title.ToLower())
                : this.Title.ToLower().Contains(other.Title.ToLower());

            // Approximate match authors since the names may be in different order
            // firstname lastname vs lastname, firstname or Dr firstname lastname
            double intersect = this.AuthorVector.Intersect(other.AuthorVector).Count();
            double union = this.AuthorVector.Union(other.AuthorVector).Count();
            var authorMatch = intersect / union;

            // Allow one extra word (e.g. Dr or PhD) for two word author names
            return titleMatch && authorMatch >= 0.4;
        }
    }
}