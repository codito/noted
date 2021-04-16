// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System;
    using System.Linq;
    using Noted.Core;

    public class ConsolePresenter : IDisposable
    {
        private readonly ExtractWorkflowEvents workflowEvents;

        public ConsolePresenter(ExtractWorkflowEvents workflowEvents)
        {
            this.workflowEvents = workflowEvents;

            this.workflowEvents.WorkflowStarted += this.WorkflowStarted;
            this.workflowEvents.WorkflowCompleted += this.WorkflowCompleted;
            this.workflowEvents.ExtractionStarted += this.ExtractionStarted;
            this.workflowEvents.ExtractionCompleted += this.ExtractionCompleted;
        }

        public void Dispose()
        {
            this.workflowEvents.WorkflowStarted -= this.WorkflowStarted;
            this.workflowEvents.WorkflowCompleted -= this.WorkflowCompleted;
            this.workflowEvents.ExtractionStarted -= this.ExtractionStarted;
            this.workflowEvents.ExtractionCompleted -= this.ExtractionCompleted;
        }

        private void WorkflowStarted(object? sender, ExtractWorkflowEvents.WorkflowStartEventArgs e)
        {
            Console.WriteLine("Started extraction...");
        }

        private void WorkflowCompleted(object? sender, ExtractWorkflowEvents.WorkflowCompleteEventArgs e)
        {
            Console.WriteLine("Completed.");
        }

        private void ExtractionStarted(object? sender, ExtractWorkflowEvents.ExtractionStartedEventArgs e)
        {
            Console.WriteLine($"  Document: {e.FileName}");
        }

        private void ExtractionCompleted(object? sender, ExtractWorkflowEvents.ExtractionCompletedEventArgs e)
        {
            Console.WriteLine($"  Completed: {e.Document.Annotations.Count()}");
        }
    }
}