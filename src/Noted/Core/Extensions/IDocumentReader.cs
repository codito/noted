// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using Noted.Core.Models;

    /// <summary>
    /// Reader provides primitives to parse a document and extract annotations
    /// and context.
    /// </summary>
    public interface IDocumentReader
    {
        /// <summary>
        /// Gets file extensions supported by this reader.
        /// </summary>
        List<string> SupportedExtensions { get; }

        /// <summary>
        /// Parse a document stream to extract embedded annotations and context for
        /// the external annotations.
        /// </summary>
        /// <param name="stream">Document stream.</param>
        /// <param name="options">Options for the reader.</param>
        /// <param name="fetchExternalAnnotations">Function to fetch external annotations for a Document.</param>
        /// <returns>Document with metadata and annotations.</returns>
        Document Read(Stream stream, ReaderOptions options, Func<DocumentReference, List<Annotation>> fetchExternalAnnotations);
    }
}