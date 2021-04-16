// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Platform.IO
{
    using System;
    using Noted.Core.Platform.IO;

    public class ConsoleLogger : ILogger
    {
        public void Debug(string format, params object?[]? arg)
        {
            Console.Out.WriteLine($"[DEBUG] {format}", arg!);
        }

        public void Error(string format, params object?[]? arg)
        {
            Console.Out.WriteLine($"[ERROR] {format}", arg!);
        }

        public void Info(string format, params object?[]? arg)
        {
            Console.Out.WriteLine($"[DEBUG] {format}", arg!);
        }
    }
}