// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// A logical component within .NET that encapsulates a set of platform dependencies.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the component.
        /// </summary>
        public ComponentType Type { get; }

        /// <summary>
        /// Gets the set of platform dependencies this component has.
        /// </summary>
        public IList<PlatformDependency> PlatformDependencies { get; } = new List<PlatformDependency>();

        /// <summary>
        /// Initializes an instance of <see cref="Component"/>.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="type">Type of the component.</param>
        public Component(string name, ComponentType type)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(name));
            }

            Name = name;
            Type = type;
        }

        /// <summary>
        /// Returns the resolved set of dependencies associated with this component.
        /// </summary>
        /// <param name="model">The <see cref="PlatformDependenciesModel"/> that contains this component.</param>
        /// <returns>The resolved set of dependencies associated with this component.</returns>
        /// <remarks>
        /// This takes into account the inheritance of dependencies from parent platforms as well as any
        /// dependency overrides.
        /// </remarks>
        public IEnumerable<PlatformDependency> GetResolvedPlatformDependencies(PlatformDependenciesModel model)
        {
            Platform containingPlatform = model.GetContainingPlatform(this);
            List<PlatformDependency> overridenDependencies = new();

            IEnumerable<Platform> platformHierarchy = new Platform[] { containingPlatform }
                .Concat(model.GetAncestorsBottomUp(containingPlatform));

            List<PlatformDependency> resolvedDependencies = new();

            List<PlatformDependency> processedDependencies = new();

            foreach (Platform platform in platformHierarchy)
            {
                Component? component = platform.Components
                    .FirstOrDefault(component => component.Name == Name && component.Type == Type);
                if (component is null)
                {
                    continue;
                }

                foreach (PlatformDependency dependency in component.PlatformDependencies)
                {
                    if (processedDependencies.Contains(dependency))
                    {
                        continue;
                    }

                    Stack<PlatformDependency> overrideHierarchy = GetOverrideHierarchy(dependency, model);
                    PlatformDependency current = overrideHierarchy.Pop();
                    processedDependencies.Add(current);
                    while (overrideHierarchy.Count > 0)
                    {
                        PlatformDependency overridingDependency = overrideHierarchy.Pop();
                        processedDependencies.Add(overridingDependency);
                        current = current.ApplyOverrides(overridingDependency);
                    }

                    resolvedDependencies.Add(current);
                }
            }

            return resolvedDependencies
                .OrderBy(dep => dep.DependencyType)
                .ThenBy(dep => dep.Name)
                .ToList();
        }

        internal void Validate(PlatformDependenciesModel model)
        {
            foreach (PlatformDependency dependency in PlatformDependencies)
            {
                dependency.Validate(model);
            }
        }

        private static Stack<PlatformDependency> GetOverrideHierarchy(PlatformDependency dependency, PlatformDependenciesModel model)
        {
            Stack<PlatformDependency> overrideHierarchy = new();
            overrideHierarchy.Push(dependency);

            PlatformDependency? overridenDependency = dependency.GetOverridenDependency(model);
            while (overridenDependency is not null)
            {
                overrideHierarchy.Push(overridenDependency);
                overridenDependency = overridenDependency.GetOverridenDependency(model);
            }

            return overrideHierarchy;
        }
    }
}
