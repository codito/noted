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
        public Configuration()
        {
            this.AnnotationProviders = [];
            this.Readers = [];
            this.Writers = [];
        }

        public int ExtractionContextLength { get; init; }

        public bool ExtractDocumentSections { get; init; } = true;

        public bool Verbose { get; init; } = false;

        public string SourcePath { get; init; } = null!;

        public string OutputPath { get; init; } = null!;

        public bool TreatSourceAsLibrary { get; init; }

        #region Dependencies

        // TODO evaluate if these should be a separate concept e.g. Environment
        public IFileSystem FileSystem { get; set; } = null!;

        public ILogger Logger { get; set; } = null!;

        public List<IAnnotationProvider> AnnotationProviders { get; set; }

        public List<IDocumentReader> Readers { get; set; }

        public List<IDocumentWriter> Writers { get; set; }

        #endregion
    }
}