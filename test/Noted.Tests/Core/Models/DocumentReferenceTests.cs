// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Core.Models
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Models;

    [TestClass]
    public class DocumentReferenceTests
    {
        [TestMethod]
        public void EqualsShouldCheckTitleAndAuthor()
        {
            var left = new DocumentReference { Title = "a", Author = "b" };
            var right = new DocumentReference { Title = "a", Author = "b" };

            Assert.AreEqual(left, right);
            Assert.IsTrue(left.Equals(right));
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        }

        [TestMethod]
        [DataRow("ab", true)]
        [DataRow("abcd", true)]
        [DataRow("acd", false)]
        [DataRow("ef", false)]
        public void IsSimilarShouldCheckTitleSubstringMatch(string title, bool similar)
        {
            var left = new DocumentReference { Title = "abc", Author = "b" };
            var right = new DocumentReference { Title = title, Author = "b" };

            Assert.AreEqual(similar, left.IsSimilar(right));
        }

        [TestMethod]
        [DataRow("Doe John", true)]
        [DataRow("Doe, John", true)]
        [DataRow("Doe, John PhD.", true)]
        [DataRow("John Doe Jr.", true)]
        [DataRow("Jane Doe", false)]
        public void IsSimilarShouldApproximateMatchAuthors(
            string author,
            bool similar)
        {
            var left = new DocumentReference { Title = "abc", Author = "John Doe" };
            var right = new DocumentReference { Title = "abc", Author = author };

            Assert.AreEqual(similar, left.IsSimilar(right));
        }
    }
}