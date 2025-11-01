﻿// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Noted.Core;
    using Noted.Core.Extensions;
    using Noted.Extensions.Libraries.Kindle;
    using Noted.Extensions.Libraries.KOReader;
    using Noted.Extensions.Readers;
    using Noted.Extensions.Writers;
    using Noted.Infra;

    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var configurationProvider = new ConfigurationProvider()
                .WithAnnotationProviders(config =>
                [
                    new ClippingAnnotationProvider(config.FileSystem, config.Logger),
                    new KOReaderAnnotationProvider(config.FileSystem, config.Logger)
                ])
                .WithReaders(config =>
                [

                    // new KfxReader(config.Logger),
                    new EpubReader(config.Logger),
                    new PdfReader(config.Logger),
                    new MobiReader(config.Logger)
                ])
                .WithWriters(config =>
                [
                    new MarkdownWriter(config.Logger)
                ]);

            var workflows = new Dictionary<string, Func<Configuration, IWorkflow>>
                { { "extract", config => new ExtractWorkflow(config.FileSystem, config.Logger) } };

            WaitForDebuggerIfEnabled();

            return await new ConsoleInterface()
                .WithArguments(args)
                .WithConfigurationProvider(configurationProvider)
                .WithWorkflows(workflows)
                .RunAsync();
        }

        private static void WaitForDebuggerIfEnabled()
        {
            var debugEnv = Environment.GetEnvironmentVariable("NOTED_DEBUG");
            if (string.IsNullOrEmpty(debugEnv))
            {
                return;
            }

            System.Console.WriteLine($"Please attach debugger to pid: {Environment.ProcessId}");
            while (!Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
    }
}
