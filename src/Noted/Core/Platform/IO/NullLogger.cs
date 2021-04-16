// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Platform.IO
{
    public class NullLogger : ILogger
    {
        public void Debug(string format, params object?[]? arg)
        {
        }

        public void Error(string format, params object?[]? arg)
        {
        }

        public void Info(string format, params object?[]? arg)
        {
        }
    }
}