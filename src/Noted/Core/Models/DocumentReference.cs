// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    public record DocumentReference
    {
        public string Title { get; init; }

        public string Author { get; init; }
    }
}