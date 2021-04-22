// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class ComponentTests
    {
        [Fact]
        public void ItSetsInitialState()
        {
            Component component = new("test", ComponentType.NuGetPackage);
            Assert.Equal("test", component.Name);
            Assert.Equal(ComponentType.NuGetPackage, component.Type);
            Assert.Empty(component.PlatformDependencies);
        }

        [Fact]
        public void ItValidatesPlatformDependencies()
        {
            Component component = CreateComponentWithUsageNotSetInPlatformDependency();

            Platform platform = new("debian");
            platform.Components.Add(component);

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);

            PlatformDependencyTests.ValidateUsageNotSetException(() => component.Validate(model));
        }

        [Fact]
        public void ItThrowsIfNameIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new Component(null!, ComponentType.NuGetPackage));
            ArgumentException ex = Assert.Throws<ArgumentException>(() => new Component(string.Empty, ComponentType.NuGetPackage));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: name", ex.Message);
        }

        [Fact]
        public void ItResolvesDependenciesInRootPlatform()
        {
            Component component = new("test", ComponentType.NuGetPackage);
            component.PlatformDependencies.Add(new PlatformDependency("dep1", DependencyType.LinuxPackage, "default"));
            component.PlatformDependencies.Add(new PlatformDependency("dep2", DependencyType.LinuxPackage, "default"));

            Platform platform = new("platform");
            platform.Components.Add(component);

            PlatformDependenciesModel model = new(new Version("2.0"));
            model.Platforms.Add(platform);

            IEnumerable<PlatformDependency> dependencies = component.GetResolvedDependencies(model);
            Assert.Equal(component.PlatformDependencies, dependencies);
        }

        [Fact]
        public void ItResolvesDependenciesInPlatformHierarchy()
        {
            Platform grandChildPlatform = new("grandChildPlatform");
            Component grandChildComponent = new("test", ComponentType.NuGetPackage);
            PlatformDependency grandChildOverridingDependency = new("dep2:13.0", DependencyType.Library, usage: "custom");
            grandChildOverridingDependency.Overrides = new DependencyRef("child-dep2", DependencyType.Library);
            grandChildComponent.PlatformDependencies.Add(grandChildOverridingDependency);
            grandChildPlatform.Components.Add(grandChildComponent);

            Platform childPlatform = new("childPlatform");
            Component childComponent = new("test", ComponentType.NuGetPackage);
            PlatformDependency childOverridingDependency = new("dep2:10.0", DependencyType.Library, usage: null, "child-dep2");
            childOverridingDependency.Overrides = new DependencyRef("dep2", DependencyType.Library);
            childComponent.PlatformDependencies.Add(childOverridingDependency);
            childComponent.PlatformDependencies.Add(new PlatformDependency("dep3", DependencyType.Library, "default"));
            childPlatform.Components.Add(childComponent);
            childPlatform.Platforms.Add(grandChildPlatform);

            Component rootComponent = new("test", ComponentType.NuGetPackage);
            rootComponent.PlatformDependencies.Add(new PlatformDependency("dep1", DependencyType.Library, "default"));
            rootComponent.PlatformDependencies.Add(new PlatformDependency("dep2", DependencyType.Library, "default"));

            // Not part of the hierarchy being tested but included to ensure it doesn't end up in the results
            Component rootComponent2 = new("test2", ComponentType.NuGetPackage);
            rootComponent2.PlatformDependencies.Add(new PlatformDependency("dep4", DependencyType.Library, "default"));

            Platform rootPlatform = new("rootPlatform");
            rootPlatform.Components.Add(rootComponent);
            rootPlatform.Components.Add(rootComponent2);
            rootPlatform.Platforms.Add(childPlatform);

            PlatformDependenciesModel model = new(new Version("2.0"));
            model.Platforms.Add(rootPlatform);

            PlatformDependencyComparer platformDependencyComparer = new();

            // Test the resolve dependencies from the grandchild
            IEnumerable<PlatformDependency> dependencies = grandChildComponent.GetResolvedDependencies(model);
            PlatformDependency[] expectedDependencies = new PlatformDependency[]
            {
                new("dep1", DependencyType.Library, "default"),
                new("dep2:13.0", DependencyType.Library, "custom"),
                new("dep3", DependencyType.Library, "default")
            };
            Assert.Equal(expectedDependencies, dependencies, platformDependencyComparer);

            // Test the resolve dependencies from the child
            dependencies = childComponent.GetResolvedDependencies(model);
            expectedDependencies = new PlatformDependency[]
            {
                new("dep1", DependencyType.Library, "default"),
                new("dep2:10.0", DependencyType.Library, "default", "child-dep2"),
                new("dep3", DependencyType.Library, "default")
            };
            Assert.Equal(expectedDependencies, dependencies, platformDependencyComparer);

            // Test the resolve dependencies from the root
            dependencies = rootComponent.GetResolvedDependencies(model);
            expectedDependencies = rootComponent.PlatformDependencies.ToArray();
            Assert.Equal(expectedDependencies, dependencies, platformDependencyComparer);
        }

        internal static Component CreateComponentWithUsageNotSetInPlatformDependency()
        {
            Component component = new("test", ComponentType.NuGetPackage);
            component.PlatformDependencies.Add(PlatformDependencyTests.CreatePlatformDependencyWithoutUsage());
            return component;
        }

        private class PlatformDependencyComparer : IEqualityComparer<PlatformDependency>
        {
            public bool Equals(PlatformDependency x, PlatformDependency y) =>
                x.DependencyType == y.DependencyType &&
                x.Id == y.Id &&
                x.Name == y.Name &&
                x.Overrides == y.Overrides &&
                x.Usage == y.Usage;

            public int GetHashCode(PlatformDependency obj) => throw new NotImplementedException();
        }
    }
}
