// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using System;

    public class InvalidClippingException : Exception
    {
        public InvalidClippingException(string error, string line, Exception innerException = null)
            : base($"{error}. Encountered above error while parsing '{line}'.", innerException)
        {
        }
    }
}