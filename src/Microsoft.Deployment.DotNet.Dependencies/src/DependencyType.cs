// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// Types of dependencies.
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// A device driver file.
        /// </summary>
        DeviceDriver,

        /// <summary>
        /// An executable file.
        /// </summary>
        Executable,

        /// <summary>
        /// A library file (e.g. Windows .dll file, Linux .so file).
        /// </summary>
        Library,

        /// <summary>
        /// A Linux package contained in a repository.
        /// </summary>
        LinuxPackage
    }
}
