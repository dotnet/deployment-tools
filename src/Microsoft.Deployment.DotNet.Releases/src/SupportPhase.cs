// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// An enumeration describing the different support phases of a <see cref="Product"/>.
    /// </summary>
    public enum SupportPhase
    {
        /// <summary>
        /// The product is considered end-of-life and will not receive any updates.
        /// </summary>
        EOL,

        /// <summary>
        /// The product is in long term support and will continue to receive updates.
        /// See the <see href="https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core">support lifecycle</see>
        /// document for details on how this value impacts the support duration.
        /// </summary>
        LTS,

        /// <summary>
        /// The product is no longer in active support and will be declared end-of-life (see <see cref="Product.EndOfLifeDate"/>).
        /// Only security fixes are provided until the product reaches end-of-life status.
        /// </summary>
        Maintenance,

        /// <summary>
        /// The product is a preview release.
        /// </summary>
        Preview,

        /// <summary>
        /// The support phase designates a release candidate.
        /// </summary>
        RC,

        /// <summary>
        /// The product is in support and will continue to receive updates. See 
        /// the <see href="https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core">support lifecycle</see>
        /// document for details on how this value impacts the support duration.
        /// </summary>
        Standard,

        /// <summary>
        /// The support phase is unrecognized.
        /// </summary>
        Unknown = 99
    }
}
