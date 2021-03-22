// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System.Threading.Tasks;

    /// <summary>
    /// Contract for all Workflows in the domain.
    /// </summary>
    public interface IWorkflow
    {
        Task<int> RunAsync(Configuration configuration);
    }
}