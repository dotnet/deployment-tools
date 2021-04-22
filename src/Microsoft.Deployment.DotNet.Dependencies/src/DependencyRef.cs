// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// A reference to a dependency.
    /// </summary>
    public class DependencyRef
    {
        /// <summary>
        /// Gets the ID of the referenced dependency.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the type of the referenced dependency.
        /// </summary>
        public DependencyType DependencyType { get; }

        /// <summary>
        /// Initializes an instance of <see cref="DependencyRef"/>.
        /// </summary>
        /// <param name="id">ID of the referenced dependency.</param>
        /// <param name="dependencyType">Type of the referenced dependency.</param>
        public DependencyRef(string id, DependencyType dependencyType)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (id == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(id));
            }

            Id = id;
            DependencyType = dependencyType;
        }

        /// <summary>
        /// Returns a value indicating whether this <see cref="DependencyRef"/> represents a reference
        /// to <paramref name="platformDependency"/>.
        /// </summary>
        /// <param name="platformDependency">The <see cref="PlatformDependency"/> to test.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="DependencyRef"/> represents a reference
        /// to <paramref name="platformDependency"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsReferenceTo(PlatformDependency platformDependency)
        {
            if (platformDependency is null)
            {
                throw new ArgumentNullException(nameof(platformDependency));
            }

            return platformDependency.Id == Id &&
                platformDependency.DependencyType == DependencyType;
        }
    }
}
