// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Tests.Infra
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Noted.Infra;

    [TestClass]
    public class ConsoleInterfaceTests
    {
        // [TestMethod]
        public async Task RunShouldTriggerExtractWorkflowWithConfiguration()
        {
            var cui = new ConsoleInterface();

            var result = await cui.RunAsync();

            Assert.AreEqual(0, result);
        }
    }
}