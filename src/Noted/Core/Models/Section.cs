// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    /// <summary>
    /// A section represents a chapter in the Document.
    /// </summary>
    public class Section
    {
        /// <summary>
        /// Title or heading for the section.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Start location of the Section relative to the Document from beginning.
        /// </summary>
        public int Location { get; set; }
    }
}