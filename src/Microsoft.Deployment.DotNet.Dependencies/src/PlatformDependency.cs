// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// Description of a platform dependency.
    /// </summary>
    public class PlatformDependency
    {
        /// <summary>
        /// Gets the ID of the dependency.
        /// </summary>
        /// <remarks>
        /// If not set, this defaults to the name portion of the dependency's name
        /// </remarks>
        public string Id { get; }

        /// <summary>
        /// Gets the raw name expression of the dependency artifact.
        /// </summary>
        /// <remarks>
        /// The name of a dependency is an expression that can be used to describe a variety of conditions. The syntax allows
        /// for version ranges to be specified using
        /// <see href="https://docs.microsoft.com/nuget/concepts/package-versioning#version-ranges">interval notation</see>.
        /// </remarks>
        /// <example>
        ///     Any version of the libgcc1 package:
        ///     <code>
        ///     libgcc1
        ///     </code>
        /// </example>
        /// <example>
        ///     Version 4.9.2 or higher of the libgcc1 package:
        ///     <code>
        ///     libgcc1:4.9.2
        ///     </code>
        /// </example>
        /// <example>
        ///     A version of the libgcc1 package that lies within the range 4.9.2 &gt;= x &lt; 5.0:
        ///     <code>
        ///     libgcc1:[4.9.2,5.0)
        ///     </code>
        /// </example>
        /// <example>
        ///     Any version of the libssl1.0.0 or libssl1.1 packages:
        ///     <code>
        ///     libssl1.0.0 || libssl1.1
        ///     </code>
        /// </example>
        public string Name { get; }

        /// <summary>
        /// Gets the parsed representation of the name expression.
        /// </summary>
        [JsonIgnore]
        public NameExpression NameExpression { get; }

        /// <summary>
        /// Gets the type of the dependency.
        /// </summary>
        public DependencyType? DependencyType { get; }

        /// <summary>
        /// Gets a value indicating in what manner the dependency is used.
        /// </summary>
        /// <remarks>
        /// This value references one of the keys in <see cref="PlatformDependenciesModel.DependencyUsages"/>
        /// </remarks>
        public string? Usage { get; }

        /// <summary>
        /// Gets or sets a reference to the dependency from the nearest parent platform that this dependency overrides.
        /// </summary>
        public DependencyRef? Overrides { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="PlatformDependency"/>.
        /// </summary>
        /// <param name="name">Name expression of the dependency artifact</param>
        /// <param name="dependencyType">Type of the dependency.</param>
        /// <param name="usage">Value indicating in what manner the dependency is used.</param>
        public PlatformDependency(string name, DependencyType dependencyType, string usage)
            : this(name, dependencyType, usage, id: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PlatformDependency"/>.
        /// </summary>
        /// <param name="name">Name expression of the dependency artifact</param>
        /// <param name="dependencyType">Type of the dependency.</param>
        /// <param name="usage">Value indicating in what manner the dependency is used.</param>
        /// <param name="id">ID of the dependency.</param>
        [JsonConstructor]
        public PlatformDependency(string name, DependencyType? dependencyType, string? usage, string? id)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(name));
            }

            NameExpression = NameExpression.Parse(name);

            if (id is null)
            {
                // Id defaults to the name portion of the name expression (excludes version info).
                if (NameExpression.Names.Count == 1)
                {
                    Id = NameExpression.Names.First().Name;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Platform dependency with name expression '{name}' needs to explicitly define its ID because " +
                        "the expression contains more than one dependency name.");
                }
            }
            else
            {
                Id = id;
            }

            Name = name;
            DependencyType = dependencyType;
            Usage = usage;
        }

        internal void Validate(PlatformDependenciesModel model)
        {
            if (Overrides is null && string.IsNullOrEmpty(Usage))
            {
                throw new FormatException("Usage must be set for platform dependencies that do not have an override set.");
            }

            if (Usage is not null && !model.DependencyUsages.ContainsKey(Usage))
            {
                throw new FormatException(
                    $"Platform dependency with type '{DependencyType}' and ID '{Id}' references a dependency usage '{Usage}' that is undefined.");
            }

            GetOverridenDependency(model);
        }

        internal PlatformDependency? GetOverridenDependency(PlatformDependenciesModel model)
        {
            if (Overrides is null)
            {
                return null;
            }

            PlatformDependency? overridenDependency = null;
            Platform parentPlatform = model.GetContainingPlatform(this);
            foreach (Platform ancestor in model.GetAncestorsBottomUp(parentPlatform))
            {
                overridenDependency = ancestor.Components
                    .SelectMany(component => component.PlatformDependencies)
                    .FirstOrDefault(dep => Overrides.IsReferenceTo(dep));
                if (overridenDependency is not null)
                {
                    break;
                }
            }

            if (overridenDependency is null)
            {
                throw new FormatException(
                    $"Platform dependency with type '{DependencyType}' and ID '{Id}' overrides a dependency with type " +
                    $"'{Overrides.DependencyType}' and ID '{Overrides.Id}' that doesn't exist in its platform hierarchy.");
            }

            return overridenDependency;
        }

        internal PlatformDependency ApplyOverrides(PlatformDependency overridingDependency) =>
            new(
                overridingDependency.Name,
                overridingDependency.DependencyType ?? DependencyType,
                overridingDependency.Usage ?? Usage,
                overridingDependency.Id);
    }
}
