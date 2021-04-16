// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System.Collections.Generic;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Platform.IO;
    using Noted.Platform.IO;

    /// <summary>
    /// Parse configuration from file system and merge with command line.
    /// </summary>
    public class ConfigurationProvider
    {
        private List<IAnnotationProvider>? annotationReaders;
        private List<IDocumentReader>? documentReaders;
        private List<IDocumentWriter>? documentWriters;
        private Configuration? commandLineConfig;

        public ConfigurationProvider WithConfiguration(Configuration config)
        {
            // TODO read configuration from file system
            // Merge with the provided commandline arguments
            this.commandLineConfig = config;
            return this;
        }

        public ConfigurationProvider WithAnnotationProviders(List<IAnnotationProvider> readers)
        {
            this.annotationReaders = readers;
            return this;
        }

        public ConfigurationProvider WithReaders(List<IDocumentReader> readers)
        {
            this.documentReaders = readers;
            return this;
        }

        public ConfigurationProvider WithWriters(List<IDocumentWriter> writers)
        {
            this.documentWriters = writers;
            return this;
        }

        public Configuration Build()
        {
            // TODO refactor the dependencies to take ILogger. Consider using
            // higher order functions for this.
            return new()
            {
                SourcePath = this.commandLineConfig!.SourcePath,
                OutputPath = this.commandLineConfig.OutputPath,
                ExtractionContextLength = this.commandLineConfig.ExtractionContextLength,
                ExtractDocumentSections = this.commandLineConfig.ExtractDocumentSections,
                Verbose = this.commandLineConfig.Verbose,
                TreatSourceAsLibrary = this.commandLineConfig.TreatSourceAsLibrary,

                Logger = this.commandLineConfig.Verbose ? new ConsoleLogger() : new NullLogger(),
                AnnotationProviders = this.annotationReaders!,
                Readers = this.documentReaders!,
                Writers = this.documentWriters!
            };
        }
    }
}