// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Core.Models
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Core.Models;

    [TestClass]
    public class LineLocationTests
    {
        [TestMethod]
        public void LineLocationCompareShouldThrowExceptionForInvalidObject()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                new LineLocation(1, 2).CompareTo(string.Empty);
            });
        }

        [TestMethod]
        public void LineLocationCompareShouldCompareLessOrGreaterValues()
        {
            var left = new LineLocation(1, 2);
            var right = new LineLocation(3, 4);

            Assert.AreEqual(-1, left.CompareTo(right));
            Assert.AreEqual(1, right.CompareTo(left));
        }

        [TestMethod]
        public void LineLocationCompareShouldCompareEqualValues()
        {
            var left = new LineLocation(1, 2);
            var right = new LineLocation(1, 2);

            Assert.AreEqual(0, left.CompareTo(right));
            Assert.AreEqual(left, right);
        }
    }
}