// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// An enumeration describing different types of releases based on their support duration.
    /// See the <see href="https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core">support lifecycle</see>
    /// documentation for further details.
    /// </summary>
    public enum ReleaseType
    {
        /// <summary>
        /// The release follows the long term support timeframe (3 years).
        /// </summary>
        LTS,

        /// <summary>
        /// This release follows the standard support timeframe (18 months).
        /// </summary>
        Standard,

        /// <summary>
        /// The release type is unknown and could not be parsed.
        /// </summary>
        Unknown = 99
    }
}
