// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Core.Platform.IO;
    using Noted.Platform.IO;

    /// <summary>
    /// Parse configuration from file system and merge with command line.
    /// </summary>
    public class ConfigurationProvider
    {
        private Func<Configuration, IEnumerable<IAnnotationProvider>> annotationReaders;
        private Func<Configuration, IEnumerable<IDocumentReader>> documentReaders;
        private Func<Configuration, IEnumerable<IDocumentWriter>> documentWriters;
        private Configuration commandLineConfig;

        public ConfigurationProvider()
        {
            this.annotationReaders = _ => Enumerable.Empty<IAnnotationProvider>();
            this.documentReaders = _ => Enumerable.Empty<IDocumentReader>();
            this.documentWriters = _ => Enumerable.Empty<IDocumentWriter>();
            this.commandLineConfig = new Configuration();
        }

        public ConfigurationProvider WithConfiguration(Configuration config)
        {
            // TODO read configuration from file system
            // Merge with the provided commandline arguments
            this.commandLineConfig = config;
            return this;
        }

        public ConfigurationProvider WithAnnotationProviders(
            Func<Configuration, IEnumerable<IAnnotationProvider>> readers)
        {
            this.annotationReaders = readers;
            return this;
        }

        public ConfigurationProvider WithReaders(
            Func<Configuration, IEnumerable<IDocumentReader>> readers)
        {
            this.documentReaders = readers;
            return this;
        }

        public ConfigurationProvider WithWriters(
            Func<Configuration, IEnumerable<IDocumentWriter>> writers)
        {
            this.documentWriters = writers;
            return this;
        }

        public Configuration Build()
        {
            this.commandLineConfig.FileSystem = new FileSystem();
            this.commandLineConfig.Logger = this.commandLineConfig.Verbose
                ? new ConsoleLogger()
                : new NullLogger();

            this.commandLineConfig.AnnotationProviders =
                this.annotationReaders(this.commandLineConfig).ToList();
            this.commandLineConfig.Readers =
                this.documentReaders(this.commandLineConfig).ToList();
            this.commandLineConfig.Writers =
                this.documentWriters(this.commandLineConfig).ToList();

            return this.commandLineConfig;
        }
    }
}