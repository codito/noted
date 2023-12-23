// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;

    /// <summary>
    /// Workflow to extract annotations from files or directory (batch mode) and
    /// write them as markdown documents.
    /// </summary>
    public class ExtractWorkflow(IFileSystem fileSystem, ILogger logger) : ExtractWorkflowEvents, IWorkflow
    {
        private readonly IFileSystem fileSystem = fileSystem;
        private readonly ILogger logger = logger;

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
        public async Task<int> RunAsync(Configuration configuration)
        {
            this.Raise(new WorkflowStartEventArgs());

            var stopWatch = Stopwatch.StartNew();

            // Extract external annotations for the library
            var externalAnnotations = configuration
                .AnnotationProviders
                .Where(provider => provider.IsAvailable(configuration.SourcePath))
                .SelectMany(provider =>
                    provider.GetAnnotations(configuration.SourcePath))
                .GroupBy(annotation => annotation.Document)
                .ToDictionary(g => g.Key);
            this.logger.Info("ExtractWorkflow: Found {0} documents with external annotations.", externalAnnotations.Count);

            var readersWithExtension = configuration
                .Readers
                .SelectMany(r => r.SupportedExtensions.Select(ext => (ext, r)))
                .ToDictionary(r => $".{r.ext}", r => r.r);
            this.logger.Info("ExtractWorkflow: {0} readers are configured.", readersWithExtension.Count);

            foreach (var file in this.EnumerateDocuments(configuration.SourcePath, readersWithExtension.Keys))
            {
                this.logger.Info("ExtractWorkflow: Processing document: {0}.", file);
                if (!readersWithExtension.TryGetValue(
                    Path.GetExtension(file),
                    out var reader))
                {
                    // Skip the file
                    this.logger.Debug("ExtractWorkflow: Skipped file as no readers are configured.");
                    continue;
                }

                // Extract if the document has _external_ annotations (kindle, koreader) or
                // can have _embedded_ annotations (like pdf).
                // FIXME
                this.Raise(new ExtractionStartedEventArgs { FileName = file });
                await using var stream = this.fileSystem.OpenPathForRead(file);
                var document = await reader.Read(
                    stream,
                    new ReaderOptions(),
                    docRef =>
                    {
                        if (externalAnnotations.TryGetValue(docRef, out var annotations))
                        {
                            return [.. annotations];
                        }

                        var key = externalAnnotations.Keys
                            .FirstOrDefault(k => k.IsSimilar(docRef));

                        if (key == null)
                        {
                            this.logger.Debug("No external annotations are found.");
                            return [];
                        }

                        return [.. externalAnnotations[key]];
                    });

                document.Source = file;
                var outputPath = await this.WriteDocument(document, configuration);
                this.Raise(new ExtractionCompletedEventArgs { Document = document, OutputPath = outputPath });
                this.logger.Info("ExtractWorkflow: Completed processing {0}.", file);
            }

            stopWatch.Stop();
            this.Raise(new WorkflowCompleteEventArgs { ElapsedTime = stopWatch.Elapsed });
            return 0;
        }

        private IEnumerable<string> EnumerateDocuments(string sourcePath, IEnumerable<string> extensions)
        {
            var supportedExtensions = $"({string.Join('|', extensions)})";
            if (this.fileSystem.IsDirectory(sourcePath))
            {
                return this.fileSystem.GetFiles(sourcePath, supportedExtensions);
            }

            return new[] { sourcePath };
        }

        private async Task<string> WriteDocument(
            Document document,
            Configuration configuration)
        {
            var writer = configuration.Writers.Single();
            var outputPath = configuration.OutputPath;
            if (configuration.TreatSourceAsLibrary || this.fileSystem.IsDirectory(outputPath))
            {
                var docName = string.IsNullOrEmpty(document.Source)
                    ? document.Title
                    : Path.GetFileNameWithoutExtension(document.Source);
                outputPath = Path.Combine(configuration.OutputPath, $"{docName}.md");
            }

            await using var stream = this.fileSystem.OpenPathForWrite(outputPath);
            await writer.Write(configuration, document, stream);

            return outputPath;
        }
    }
}