// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries
{
    using System.Collections.Generic;
    using System.Linq;
    using Noted.Core.Extensions;
    using Noted.Core.Models;

    public class KOReaderAnnotationProvider : IAnnotationProvider
    {
        public bool IsAvailable(string sourcePath) => false;

        public IEnumerable<Annotation> GetAnnotations(string sourcePath)
        {
            // TODO see https://github.com/noembryo/KoHighlights/blob/0fedce43f88cdf6c8726b3cab6cb5cb1a74e815b/main.py
            return Enumerable.Empty<Annotation>();
        }
    }
}