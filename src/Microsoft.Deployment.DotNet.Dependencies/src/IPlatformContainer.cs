// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    internal interface IPlatformContainer
    {
        IList<Platform> Platforms { get; }
    }
}
