// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System.Collections.Generic;
    using Noted.Core.Extensions;
    using Noted.Core.Platform.IO;

    /// <summary>
    /// Configuration for the app.
    /// </summary>
    public class Configuration
    {
        public byte ExtractionContextLength { get; init; }

        public bool ExtractDocumentSections { get; init; } = true;

        public bool Verbose { get; init; } = false;

        public string SourcePath { get; init; } = null!;

        public string OutputPath { get; init; } = null!;

        public bool TreatSourceAsLibrary { get; init; }

#region Dependencies

        public List<IAnnotationProvider> AnnotationProviders { get; init; } = null!;

        public ILogger Logger { get; init; } = null!;

        public List<IDocumentReader> Readers { get; init; } = null!;

        public List<IDocumentWriter> Writers { get; init; } = null!;

#endregion
    }
}