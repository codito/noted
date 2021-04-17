// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.Kindle
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Noted.Core.Extensions;
    using Noted.Core.Models;
    using Noted.Core.Platform.IO;

    public class ClippingAnnotationProvider : IAnnotationProvider
    {
        private const string ClippingsFile = "My Clippings.txt";
        private readonly IFileSystem fileSystem;
        private readonly ILogger logger;

        public ClippingAnnotationProvider(
            IFileSystem fileSystem,
            ILogger logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        public bool IsAvailable(string sourcePath)
        {
            var annotationFile = Path.Combine(sourcePath, ClippingsFile);
            return this.fileSystem.IsDirectory(sourcePath) &&
                   this.fileSystem.Exists(annotationFile);
        }

        public IEnumerable<Annotation> GetAnnotations(string sourcePath)
        {
            if (!this.IsAvailable(sourcePath))
            {
                return Enumerable.Empty<Annotation>();
            }

            var annotationFile = Path.Combine(sourcePath, ClippingsFile);
            using var stream = this.fileSystem.OpenPathForRead(annotationFile);
            return ClippingParser.Parse(stream).Select(c => c.ToAnnotation()).ToList();
        }
    }
}