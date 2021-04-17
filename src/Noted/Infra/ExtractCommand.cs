// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Threading.Tasks;
    using Noted.Core;
    using Spectre.Console;

    public static class ExtractCommand
    {
        public static RootCommand Create(ConfigurationProvider configurationProvider, Func<Configuration, IWorkflow> createWorkflow)
        {
            var contextOption = new Option<byte>(
                new[] { "-c", "--context" },
                () => 0,
                "extract <context> lines of text before and after the annotation");
            var tocOption = new Option<bool>(
                new[] { "-t", "--toc" },
                () => true,
                "extract table of contents and align annotations");
            var verboseOption = new Option<bool>(
                new[] { "-v", "--verbose" },
                () => false,
                "enable verbose logging");

            var rootCommand = new RootCommand
            {
                contextOption,
                tocOption,
                verboseOption,
                new Argument<string>(
                    "sourcePath",
                    "Source document or directory of documents to extract annotations"),
                new Argument<string>(
                    "outputPath",
                    "Destination file or directory")
            };
            rootCommand.Description = "Extracts highlights and notes from documents and save them as markdown";

            rootCommand.Handler = CommandHandler.Create<ExtractCommandArguments>(
                async args => await ExecuteAsync(
                    args,
                    configurationProvider,
                    createWorkflow));

            return rootCommand;
        }

        private static async Task<int> ExecuteAsync(
            ExtractCommandArguments args,
            ConfigurationProvider configurationProvider,
            Func<Configuration, IWorkflow> createWorkflow)
        {
            try
            {
                if (!File.Exists(args.SourcePath) &&
                    !Directory.Exists(args.SourcePath))
                {
                    AnsiConsole.MarkupLine($"[bold]SourcePath[/] must be a valid file or directory. You provided [red]{args.SourcePath}[/].");
                    return 1;
                }

                // Source as library considers the input path as a Kindle or similar
                // library. All documents will be considered for extraction.
                var sourceAsLibrary = File.GetAttributes(args.SourcePath)
                    .HasFlag(FileAttributes.Directory);
                if (sourceAsLibrary && !Directory.Exists(args.OutputPath))
                {
                    AnsiConsole.MarkupLine($"[bold]OutputPath[/] must be a valid directory. You provided [red]{args.OutputPath}[/].");
                    return 1;
                }

                var configuration = configurationProvider
                    .WithConfiguration(new Configuration
                    {
                        ExtractionContextLength = args.Context,
                        ExtractDocumentSections = args.ExtractDocumentSections,
                        Verbose = args.Verbose,
                        SourcePath = args.SourcePath,
                        OutputPath = args.OutputPath,
                        TreatSourceAsLibrary = sourceAsLibrary
                    }).Build();

                var workflow = createWorkflow(configuration);
                using var consolePresenter = new ConsolePresenter((ExtractWorkflowEvents)workflow);
                return await workflow.RunAsync(configuration);
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[red]✗ Aborted.[/]");
                return -1;
            }
        }

        private class ExtractCommandArguments
        {
            public byte Context { get; set; }

            public bool ExtractDocumentSections { get; set; }

            public string SourcePath { get; set; } = null!;

            public string OutputPath { get; set; } = null!;

            public bool Verbose { get; set; }
        }
    }
}