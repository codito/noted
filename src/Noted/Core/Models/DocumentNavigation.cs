// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    /// <summary>
    /// Represents a navigation element in the Document. E.g. a chapter,
    /// or sections within a chapter.
    /// </summary>
    public class DocumentNavigation
    {
        /// <summary>
        /// Gets or sets the title or heading for the section.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the level of the navigation element.
        /// </summary>
        /// <remarks>Imagine a table of contents. A top level element has level 1.
        /// Sections within it are level 2 and so on.</remarks>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the start location of the Section relative to the Document from beginning.
        /// </summary>
        public int Location { get; set; }
    }
}