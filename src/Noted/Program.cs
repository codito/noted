// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Extensions.Libraries;
    using Noted.Extensions.Libraries.Kindle;
    using Noted.Extensions.Readers;
    using Noted.Extensions.Writers;
    using Noted.Infra;
    using Noted.Platform.IO;

    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // Initialize a Configuration instance
            // Register the extensions
            // Start ExtractWorkflow
            var fileSystem = new FileSystem();
            var configurationProvider = new ConfigurationProvider()
                .WithAnnotationProviders(new List<IAnnotationProvider>
                {
                    new ClippingAnnotationProvider(fileSystem),
                    new KOReaderAnnotationProvider()
                })
                .WithReaders(new List<IDocumentReader>
                {
                    new PdfReader(),
                    new EpubReader(),
                    new MobiReader()
                })
                .WithWriters(new List<IDocumentWriter>
                {
                    new MarkdownWriter()
                });

            return await new ConsoleInterface()
                .WithArguments(args)
                .WithConfigurationProvider(configurationProvider)
                .WithWorkflows(new Dictionary<string, IWorkflow> { { "default", new ExtractWorkflow(fileSystem) } })
                .RunAsync();
        }
    }
}
