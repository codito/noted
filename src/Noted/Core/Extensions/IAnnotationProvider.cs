// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Extensions
{
    using System.Collections.Generic;
    using Noted.Core.Models;

    /// <summary>
    /// Annotation providers extract external annotations for a library or a document.
    /// E.g. Kindle stores annotations in <c>My Clippings.txt</c>, or KOReader
    /// stores per document annotations externally in <c>sdr</c> directories.
    /// </summary>
    public interface IAnnotationProvider
    {
        /// <summary>
        /// Returns true if this provider supports given file or directory path.
        /// </summary>
        /// <param name="sourcePath">Path to directory or a document.</param>
        /// <returns>True if provider is supported.</returns>
        bool IsAvailable(string sourcePath);

        /// <summary>
        /// Gets annotations from the external source.
        /// </summary>
        /// <param name="sourcePath">Path to directory or a document.</param>
        /// <returns>List of external annotations.</returns>
        IEnumerable<Annotation> GetAnnotations(string sourcePath);
    }
}