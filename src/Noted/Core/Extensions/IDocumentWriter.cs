// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Extensions
{
    using System.IO;
    using System.Threading.Tasks;
    using Noted.Core.Models;

    public interface IDocumentWriter
    {
        Task Write(Configuration configuration, Document document, Stream output);
    }
}