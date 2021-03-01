// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file in the project root for full license information.

namespace Noted.Core.Models
{
    using System.Collections.Generic;

    public record Document : DocumentReference
    {
        public string Subject { get; set; }

        public string Keywords { get; set; }

        public string ModifiedDate { get; set; }

        public string Source { get; set; }

        public IEnumerable<Annotation> Annotations { get; set; }
    }
}