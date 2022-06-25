// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Releases
{
    /// <summary>
    /// An enumeration describing the different types of releases and support phases of a <see cref="Product"/>.
    /// </summary>
    [JsonConverter(typeof(ReleaseLabelConverter))]
    public enum ReleaseLabel
    {
        /// <summary>
        /// The product is considered end-of-life and will not receive any updates.
        /// </summary>
        EOL,

        /// <summary>
        /// The product is supported for production applications.
        /// </summary>
        GoLive,

        /// <summary>
        /// The product will receive long-term support..
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
        /// The product will receive short-term support.
        /// </summary>
        STS,

        /// <summary>
        /// The support policy is unrecognized.
        /// </summary>
        Unknown = 99
    }
}
