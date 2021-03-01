// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Extensions
{
    using System.IO;
    using Noted.Core.Models;

    public interface IDocumentWriter
    {
        void Write(Document document, Stream output);
    }
}