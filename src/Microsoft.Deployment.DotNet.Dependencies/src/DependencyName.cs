// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// Represents a reference to a dependency name and optional version information.
    /// </summary>
    public class DependencyName
    {
        /// <summary>
        /// Gets the name of the dependency.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the version range information for the dependency.
        /// </summary>
        public VersionRange? VersionRange { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DependencyName"/>.
        /// </summary>
        /// <param name="name">Name of the dependency.</param>
        public DependencyName(string name)
        {
            Name = name;
        }

        internal static DependencyName Parse(string dependencyName)
        {
            if (dependencyName == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(dependencyName));
            }

            int versionRangeSeparatorIndex = dependencyName.IndexOf(':');
            if (versionRangeSeparatorIndex > 0)
            {
                return new DependencyName(dependencyName.Substring(0, versionRangeSeparatorIndex))
                {
                    VersionRange = VersionRange.Parse(dependencyName.Substring(versionRangeSeparatorIndex + 1))
                };
            }
            else
            {
                return new DependencyName(dependencyName);
            }
        }
    }
}
