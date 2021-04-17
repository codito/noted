// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Infra
{
    using System;
    using System.Linq;
    using Noted.Core;
    using Spectre.Console;

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
        }

        private void WorkflowCompleted(object? sender, ExtractWorkflowEvents.WorkflowCompleteEventArgs e)
        {
            AnsiConsole.MarkupLine("[bold green]Completed in {0:0.00}s.[/]", e.ElapsedTime.TotalSeconds);
        }

        private void ExtractionStarted(object? sender, ExtractWorkflowEvents.ExtractionStartedEventArgs e)
        {
            AnsiConsole.MarkupLine($"[bold olive]>[/] Extracting [blue]{e.FileName}[/]");
        }

        private void ExtractionCompleted(object? sender, ExtractWorkflowEvents.ExtractionCompletedEventArgs e)
        {
            var authorMsg = string.IsNullOrEmpty(e.Document.Author)
                ? string.Empty
                : $" by [aqua]{e.Document.Author}[/]";
            var sectionMsg = e.Document.Sections.Any()
                ? $" in [aqua]{e.Document.Sections.Count()}[/] sections"
                : string.Empty;

            // AnsiConsole.Cursor.SetPosition(0, Console.CursorTop);
            AnsiConsole.MarkupLine($" [bold green]✓[/] [aqua]{e.Document.Title}[/]{authorMsg}");
            AnsiConsole.MarkupLine($" [bold green]✓[/] [aqua]{e.Document.Annotations.Count()}[/] annotations{sectionMsg}");
            AnsiConsole.MarkupLine($" [bold green]✓[/] Saved to [blue]{e.OutputPath}[/]");
            AnsiConsole.MarkupLine(string.Empty);
        }
    }
}