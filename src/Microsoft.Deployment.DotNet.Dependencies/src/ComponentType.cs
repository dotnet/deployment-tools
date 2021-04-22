// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// Types of .NET components.
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// A git repository (e.g. https://github.com/dotnet/runtime.git). This is used when needing to describe that a .NET
        /// repository has platform dependencies in order to build it.
        /// </summary>
        GitRepository,

        /// <summary>
        /// A shared framework (e.g. Microsoft.NETCore.App, Microsoft.AspNetCore.App).
        /// </summary>
        SharedFramework,

        /// <summary>
        /// A NuGet package (e.g. System.Drawing.Common).
        /// </summary>
        NuGetPackage
    }
}
