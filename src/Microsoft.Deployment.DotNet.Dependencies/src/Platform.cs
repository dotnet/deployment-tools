// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// A supported platform.
    /// </summary>
    public class Platform : IPlatformContainer
    {
        /// <summary>
        /// Gets the RID identifying the platform.
        /// </summary>
        public string Rid { get; }

        /// <summary>
        /// Gets the inheriting child platforms of this platform.
        /// </summary>
        /// <remarks>
        /// These child platforms are typically version- or arch-specific variations of the base platform.
        /// They inherit the characteristics of their parent hierarchy but can override them.
        /// </remarks>
        public IList<Platform> Platforms { get; } = new List<Platform>();

        /// <summary>
        /// Gets the set of components and their dependencies specific to this platform.
        /// </summary>
        public IList<Component> Components { get; } = new List<Component>();

        /// <summary>
        /// Initializes a new instance of <see cref="Platform"/>.
        /// </summary>
        /// <param name="rid">RID identifying the platform.</param>
        public Platform(string rid)
        {
            if (rid is null)
            {
                throw new ArgumentNullException(nameof(rid));
            }

            if (rid == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(rid));
            }

            Rid = rid;
        }

        internal void Validate(PlatformDependenciesModel model)
        {
            HashSet<(string Name, ComponentType Type)> components = new();
            foreach (Component component in Components)
            {
                if (components.Contains((component.Name, component.Type)))
                {
                    throw new FormatException(
                        $"Detected duplicate components in platform '{Rid}'. Each component's name and type " +
                        "should represent a unique identity within a platform.");
                }

                components.Add((component.Name, component.Type));
                component.Validate(model);
            }

            foreach (Platform platform in Platforms)
            {
                platform.Validate(model);
            }
        }

        /// <summary>
        /// Fills <paramref name="ancestors"/> with the set of platforms that are ancestors of <paramref name="childPlatform"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="childPlatform"/> is not a descendant of this platform then no platforms will be added to
        /// <paramref name="childPlatform"/>.
        /// </remarks>
        internal void AddAncestorsBottomUp(Platform childPlatform, List<Platform> ancestors)
        {
            foreach (Platform platform in Platforms)
            {
                if (platform == childPlatform)
                {
                    ancestors.Add(this);
                    return;
                }

                platform.AddAncestorsBottomUp(childPlatform, ancestors);
                if (ancestors.Any())
                {
                    ancestors.Add(this);
                    return;
                }
            }
        }
    }
}
