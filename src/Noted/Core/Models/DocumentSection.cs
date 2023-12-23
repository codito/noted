// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    /// <summary>
    /// Represents a navigation element in the Document. E.g. a chapter,
    /// or sections within a chapter.
    /// </summary>
    public record DocumentSection(
        string Title,
        int Level,
        int Location,
        DocumentSection? Parent);
}