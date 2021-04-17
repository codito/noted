// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Platform.IO
{
    using Noted.Core.Platform.IO;
    using Spectre.Console;

    public class ConsoleLogger : ILogger
    {
        public void Debug(string format, params object?[]? arg)
        {
            AnsiConsole.MarkupLine($"[grey]DEBUG: {format}[/]", arg!);
        }

        public void Error(string format, params object?[]? arg)
        {
            AnsiConsole.MarkupLine($"[red]ERROR: {format}[/]", arg!);
        }

        public void Info(string format, params object?[]? arg)
        {
            AnsiConsole.MarkupLine($"[grey]INFO: {format}[/]", arg!);
        }
    }
}