// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;
    using Noted.Extensions.Writers;
    using Noted.Platform.IO;

    /// <summary>
    /// Workflow to extract annotations from files or directory (batch mode) and
    /// write them as markdown documents.
    /// </summary>
    public class ExtractWorkflow : IWorkflow
    {
        private readonly IFileSystem fileSystem;

        public ExtractWorkflow(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        // Configurations
        //  - Input: Single file or Directory (batch mode)
        //  - Output: Single file or Directory
        //      - File format: markdown (default)
        //      - Template and variables
        //  - Extraction
        //      - Context length = 3 (default)
        //
        // Workflow steps
        //  1. Create a Document instance for the input documents
        //  2. Extract global annotations (kindle) and group them by Document
        //  3. Walk through each Document
        //      3.1 Extract external annotations (koreader)
        //      3.2 Extract embedded annotations and context
        //          3.2.1 Use location scheme provided in external annotation
        //          3.2.2 Extract surround text as context
        //          3.2.3 Extract bookmarks and headers as context
        //  4. Create AnnotationCollection for each Document
        //  5. Export annotations to output
        public Task<int> RunAsync(Configuration configuration)
        {
            // Extract external annotations for the library
            var externalAnnotations = configuration
                .AnnotationProviders
                .Where(provider => provider.IsAvailable(configuration.SourcePath))
                .SelectMany(provider =>
                    provider.GetAnnotations(configuration.SourcePath))
                .GroupBy(annotation => annotation.Document)
                .ToDictionary(g => g.Key);

            var readersWithExtension = configuration
                .Readers
                .SelectMany(r => r.SupportedExtensions.Select(ext => (ext, r)))
                .ToDictionary(r => $".{r.ext}", r => r.r);

            foreach (var file in this.EnumerateDocuments(configuration.SourcePath))
            {
                if (!readersWithExtension.TryGetValue(
                    Path.GetExtension(file),
                    out var reader))
                {
                    // Skip the file
                    continue;
                }

                // TODO extract file scoped external annotations (KOReader)
                using var stream = this.fileSystem.OpenPathForRead(file);
                var document = reader.Read(
                    stream,
                    new ReaderOptions(),
                    docRef =>
                    {
                        if (externalAnnotations.TryGetValue(docRef, out var annotations))
                        {
                            return annotations.ToList();
                        }

                        return new List<Annotation>();
                    });

                this.WriteDocument(document, configuration);
            }

            return Task.FromResult(0);
        }

        private IEnumerable<string> EnumerateDocuments(string sourcePath)
        {
            var supportedExtensions = "(pdf|epub|azw3|mobi)";
            if (this.fileSystem.IsDirectory(sourcePath))
            {
                return this.fileSystem.GetFiles(sourcePath, supportedExtensions);
            }

            return new[] { sourcePath };
        }

        private void WriteDocument(
            Document document,
            Configuration configuration)
        {
            var writer = configuration.Writers.Single();
            var outputPath = configuration.OutputPath;
            if (configuration.TreatSourceAsLibrary)
            {
                outputPath = Path.Combine(configuration.OutputPath, $"{document.Title}.md");
            }

            using var stream = this.fileSystem.OpenPathForWrite(outputPath);
            writer.Write(document, stream);
            Console.WriteLine($"Completed: {document.Title}");
        }
    }
}