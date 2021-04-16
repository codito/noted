// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Noted.Core;
    using Noted.Core.Platform.IO;
    using Noted.Platform.IO;

    /// <summary>
    /// Entrypoint for the Console User Interface.
    /// </summary>
    public class ConsoleInterface
    {
        private string[] arguments = null!;
        private ConfigurationProvider configurationProvider = null!;
        private IDictionary<string, IWorkflow> workflows = null!;

        public ConsoleInterface WithArguments(string[] args)
        {
            this.arguments = args;
            return this;
        }

        public ConsoleInterface WithConfigurationProvider(ConfigurationProvider provider)
        {
            this.configurationProvider = provider;
            return this;
        }

        public ConsoleInterface WithWorkflows(IDictionary<string, IWorkflow> workflows)
        {
            this.workflows = workflows;
            return this;
        }

        public Task<int> RunAsync()
        {
            var contextOption = new Option<byte>(
                "--context",
                () => 0,
                "extract <context> lines of text before and after the annotation");
            contextOption.AddAlias("-c");
            var tocOption = new Option<bool>(
                "--toc",
                () => true,
                "extract table of contents and align annotations");
            tocOption.AddAlias("-t");
            var verboseOption = new Option<bool>(
                "--verbose",
                () => false,
                "enable verbose logging");
            verboseOption.AddAlias("-v");

            var rootCommand = new RootCommand
            {
                contextOption,
                tocOption,
                new Argument<string>(
                    "sourcePath",
                    "Source document or directory of documents to extract annotations"),
                new Argument<string>(
                    "outputPath",
                    "Destination file or directory")
            };

            // rootCommand.Name = "extract";
            rootCommand.Description = "Extracts highlights and notes from documents and save them as markdown";
            rootCommand.Handler = CommandHandler.Create<CommandLineArguments>(cliArgs =>
            {
                try
                {
                    var configuration = this.configurationProvider
                        .WithConfiguration(cliArgs.ToConfiguration())
                        .Build();
                    return this.workflows.Single().Value
                        .RunAsync(configuration);
                }
                catch (ArgumentException e)
                {
                    Console.Error.WriteLine($"Required argument is not provided: {e.Message}.");
                    return Task.FromResult(1);
                }
                catch (OperationCanceledException)
                {
                    Console.Error.WriteLine("Aborted current operation.");
                    return Task.FromResult(-1);
                }
            });

            return rootCommand.InvokeAsync(this.arguments);
        }

        private class CommandLineArguments
        {
            public byte Context { get; set; }

            public bool ExtractDocumentSections { get; set; }

            public string SourcePath { get; set; } = null!;

            public string OutputPath { get; set; } = null!;

            public bool Verbose { get; set; }

            public Configuration ToConfiguration()
            {
                if (!File.Exists(this.SourcePath) && !Directory.Exists(this.SourcePath))
                {
                    throw new ArgumentException(nameof(this.SourcePath));
                }

                // Source as library considers the input path as a Kindle or similar
                // library. All documents will be considered for extraction.
                var sourceAsLibrary = File.GetAttributes(this.SourcePath)
                    .HasFlag(FileAttributes.Directory);
                if (sourceAsLibrary && !Directory.Exists(this.OutputPath))
                {
                    throw new ArgumentException(nameof(this.OutputPath));
                }

                return new()
                {
                    ExtractionContextLength = this.Context,
                    ExtractDocumentSections = this.ExtractDocumentSections,
                    Verbose = this.Verbose,
                    SourcePath = this.SourcePath,
                    OutputPath = this.OutputPath,
                    TreatSourceAsLibrary = sourceAsLibrary
                };
            }
        }
    }
}