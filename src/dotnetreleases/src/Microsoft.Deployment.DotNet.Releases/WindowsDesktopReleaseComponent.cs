// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// Represents a Windows Desktop runtime release.
    /// </summary>
    public class WindowsDesktopReleaseComponent : ReleaseComponent
    {
        internal WindowsDesktopReleaseComponent(JToken token, ProductRelease release) : base(token, release)
        {
            Name = ReleasesResources.WindowsDesktopReleaseName;
        }
    }
}
