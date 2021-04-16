// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core
{
    using System;
    using Noted.Core.Models;

    public class ExtractWorkflowEvents
    {
        public event EventHandler<WorkflowStartEventArgs>? WorkflowStarted;

        public event EventHandler<WorkflowCompleteEventArgs>? WorkflowCompleted;

        public event EventHandler<ExtractionStartedEventArgs>? ExtractionStarted;

        public event EventHandler<ExtractionCompletedEventArgs>? ExtractionCompleted;

        protected void Raise<T>(T e)
            where T : EventArgs
        {
            switch (e)
            {
                case WorkflowStartEventArgs w:
                    this.WorkflowStarted?.Invoke(this, w);
                    break;
                case WorkflowCompleteEventArgs w:
                    this.WorkflowCompleted?.Invoke(this, w);
                    break;
                case ExtractionStartedEventArgs w:
                    this.ExtractionStarted?.Invoke(this, w);
                    break;
                case ExtractionCompletedEventArgs w:
                    this.ExtractionCompleted?.Invoke(this, w);
                    break;
            }
        }

        public class WorkflowStartEventArgs : EventArgs
        {
        }

        public class WorkflowCompleteEventArgs : EventArgs
        {
        }

        public class ExtractionStartedEventArgs : EventArgs
        {
            public string FileName { get; init; } = null!;
        }

        public class ExtractionCompletedEventArgs : EventArgs
        {
            public Document Document { get; init; } = null!;
        }
    }
}