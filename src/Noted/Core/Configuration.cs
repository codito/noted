// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System.Collections.Generic;
    using Noted.Core.Extensions;

    /// <summary>
    /// Configuration for the app.
    /// </summary>
    public class Configuration
    {
        public byte ExtractionContextLength { get; init; } = 0;

        public byte ExtractionHeaderDepth { get; init; } = 3;

        public string SourcePath { get; init; }

        public string OutputPath { get; init; }

        public bool TreatSourceAsLibrary { get; init; } = false;

#region Dependencies
        public List<IAnnotationProvider> AnnotationProviders
        {
            get;
            init;
        }

        public List<IDocumentReader> Readers { get; init; }

        public List<IDocumentWriter> Writers { get; init; }
#endregion
    }
}