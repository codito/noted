// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
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
        public async Task<int> RunAsync(Configuration configuration)
        {
            this.Raise(new WorkflowStartEventArgs());

            var flowStopWatch = Stopwatch.StartNew();

            // Extract external annotations for the library - koreader, kindle etc.
            var stepTimer = Stopwatch.StartNew();
            var externalAnnotations = configuration
                .AnnotationProviders
                .Where(provider => provider.IsAvailable(configuration.SourcePath))
                .SelectMany(provider =>
                    provider.GetAnnotations(configuration.SourcePath))
                .GroupBy(annotation => annotation.Document)
                .ToDictionary(g => g.Key);
            stepTimer.Stop();
            this.logger.Info("ExtractWorkflow: Found {0} documents with external annotations. Time: {1:0.00}s", externalAnnotations.Count, stepTimer.Elapsed.TotalSeconds);

            // Find the readers available and group them by the supported file extensions
            var readersWithExtension = configuration
                .Readers
                .SelectMany(r => r.SupportedExtensions.Select(ext => (ext, r)))
                .ToDictionary(r => $".{r.ext}", r => r.r);
            this.logger.Info("ExtractWorkflow: {0} readers are configured.", readersWithExtension.Count);

            // Fetch metadata for the documents in library. We'll use this for aligning with
            // the annotations in next step.
            stepTimer.Restart();
            var documentLibrary = new Dictionary<DocumentReference, IDocumentReader>();
            this.logger.Info("ExtractWorkflow: Parsing document metadata.");
            foreach (var file in this.EnumerateDocuments(configuration.SourcePath, readersWithExtension.Keys))
            {
                this.logger.Info("ExtractWorkflow: Processing document: {0}.", file);
                if (!readersWithExtension.TryGetValue(
                    Path.GetExtension(file),
                    out var reader))
                {
                    // Skip the file
                    this.logger.Debug($"ExtractWorkflow: Skipped file as no readers are configured. File: {file}");
                    continue;
                }

                await using var stream = this.fileSystem.OpenPathForRead(file);
                var docRef = await reader.GetMetadata(stream);
                docRef.Path = file;
                documentLibrary.Add(docRef, reader);
            }

            stepTimer.Stop();
            this.logger.Info("ExtractWorkflow: Found {0} documents in library. Time: {1:0.00}s.", documentLibrary.Count, stepTimer.Elapsed.TotalSeconds);

            // Align external annotations with the documents
            this.logger.Info("ExtractWorkflow: Aligning external annotations with documents in library.");
            stepTimer.Restart();
            foreach (var (annotationDocRef, annotationGroup) in externalAnnotations)
            {
                var (docRef, reader) = documentLibrary.Where(kv => kv.Key.IsSimilar(annotationDocRef)).FirstOrDefault();
                if (docRef is null || reader is null)
                {
                    this.logger.Debug("ExtractWorkflow: Couldn't find a matching document in library. Annotation doc reference: {0}. {1} annotations.", annotationDocRef, annotationGroup.Count());
                    continue;
                }

                await this.ParseAnnotationsInDocument(configuration, docRef, reader, []);
            }

            stepTimer.Stop();
            this.logger.Info("ExtractWorkflow: Aligned {0} documents in library. Time: {1:0.00}s.", externalAnnotations.Keys.Count, stepTimer.Elapsed.TotalSeconds);

            // Fetch embedded annotations for documents in library
            this.logger.Info("ExtractWorkflow: Extract embedded annotations from document library");
            stepTimer.Restart();
            foreach (var (docRef, reader) in documentLibrary.Where(d => d.Key.SupportsEmbeddedAnnotation))
            {
                // FIXME support pdf documents with external annotations
                await this.ParseAnnotationsInDocument(configuration, docRef, reader, []);
            }

            this.logger.Info("ExtractWorkflow: Completed embedded annotation flow. Time: {0:0.00}s.", stepTimer.Elapsed.TotalSeconds);
            stepTimer.Stop();

            flowStopWatch.Stop();
            this.Raise(new WorkflowCompleteEventArgs { ElapsedTime = flowStopWatch.Elapsed });
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

        private async Task ParseAnnotationsInDocument(Configuration configuration, DocumentReference docRef, IDocumentReader reader, List<Annotation> annotations)
        {
            var file = docRef.Path;
            var stream = this.fileSystem.OpenPathForRead(file);

            this.Raise(new ExtractionStartedEventArgs { FileName = file });
            var document = await reader.Read(
                stream,
                new ReaderOptions(),
                annotations);

            document.Source = file;
            var outputPath = await this.WriteDocument(document, configuration);
            this.Raise(new ExtractionCompletedEventArgs { Document = document, OutputPath = outputPath });
            this.logger.Info("ExtractWorkflow: Completed processing {0}.", file);
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