// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Threading.Tasks;
    using Noted.Core;

    /// <summary>
    /// Entrypoint for the Console User Interface.
    /// </summary>
    public class ConsoleInterface
    {
        private string[] arguments;
        private ConfigurationProvider configurationProvider;
        private Dictionary<string, Func<Configuration, IWorkflow>> workflowFactory;

        public ConsoleInterface()
        {
            this.arguments = Array.Empty<string>();
            this.configurationProvider = new ConfigurationProvider();
            this.workflowFactory = new Dictionary<string, Func<Configuration, IWorkflow>>();
        }

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

        public ConsoleInterface WithWorkflows(Dictionary<string, Func<Configuration, IWorkflow>> createWorkflow)
        {
            this.workflowFactory = createWorkflow;
            return this;
        }

        public Task<int> RunAsync()
        {
            return ExtractCommand
                .Create(
                    this.configurationProvider,
                    this.workflowFactory.Single().Value)
                .InvokeAsync(this.arguments);
        }
    }
}