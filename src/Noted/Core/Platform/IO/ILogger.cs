// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Platform.IO
{
    /// <summary>
    /// A simple logging and information output abstraction.
    /// </summary>
    public interface ILogger
    {
        void Debug(string format, params object?[]? arg);

        void Error(string format, params object?[]? arg);

        void Info(string format, params object?[]? arg);
    }
}