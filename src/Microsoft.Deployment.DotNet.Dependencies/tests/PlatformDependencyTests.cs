// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class PlatformDependencyTests
    {
        [Theory]
        [InlineData("name", "usage", null)]
        [InlineData("name", "usage", "id")]
        public void ItSetsInitialState(string name, string usage, string? id)
        {
            PlatformDependency platformDependency = id is null ?
                new(name, DependencyType.LinuxPackage, usage) : new(name, DependencyType.LinuxPackage, usage, id);

            Assert.Equal(name, platformDependency.Name);
            Assert.IsType<DependencyName>(platformDependency.ParsedName);
            DependencyName depName = (DependencyName)platformDependency.ParsedName;
            Assert.Equal(name, depName.Name);
            Assert.Null(depName.VersionRange);
            Assert.Equal(usage, platformDependency.Usage);
            Assert.Null(platformDependency.Overrides);

            string expectedId = id is null ? name : id;
            Assert.Equal(expectedId, platformDependency.Id);
        }

        [Fact]
        public void ItThrowsIfNameIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new PlatformDependency(null!, DependencyType.LinuxPackage, "usage"));
            ArgumentException ex = Assert.Throws<ArgumentException>(
                () => new PlatformDependency(string.Empty, DependencyType.LinuxPackage, "usage"));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: name", ex.Message);
        }

        [Fact]
        public void ItThrowsIfIdNotExplicitlySetForNameWithBinaryExpression()
        {
            const string name = "name1 || name2";
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => new PlatformDependency(name, DependencyType.LinuxPackage, "usage"));

            Assert.Equal(
                $"Platform dependency with name expression '{name}' needs to explicitly define its ID because " +
                "the name is not a simple expression.", ex.Message);
        }

        [Fact]
        public void ItThrowsIfUsageIsNotSet()
        {
            PlatformDependency platformDependency = CreatePlatformDependencyWithoutUsage();

            Component component = new("test", ComponentType.NuGetPackage);
            component.PlatformDependencies.Add(platformDependency);

            Platform platform = new("debian");
            platform.Components.Add(component);

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);

            ValidateUsageNotSetException(() => platformDependency.Validate(model));
        }

        [Fact]
        public void ItThrowsIfReferencedUsageIsNotDefined()
        {
            PlatformDependency platformDependency = new("name", DependencyType.LinuxPackage, "usage");

            Component component = new("component", ComponentType.SharedFramework);
            component.PlatformDependencies.Add(platformDependency);

            Platform platform = new("rid");
            platform.Components.Add(component);

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);

            FormatException ex = Assert.Throws<FormatException>(() => platformDependency.Validate(model));
            Assert.Equal(
                $"Platform dependency with type '{DependencyType.LinuxPackage}' and ID 'name' references a dependency usage 'usage' that is undefined.",
                ex.Message);
        }

        [Fact]
        public void ItThrowsIfOverridesDoesNotResolve()
        {
            PlatformDependency platformDependency = new("name", DependencyType.Executable, "usage")
            {
                Overrides = new DependencyRef("bad_ref", DependencyType.Executable)
            };
            Component component = new("component", ComponentType.SharedFramework);
            component.PlatformDependencies.Add(platformDependency);

            Platform platform = new("rid");
            platform.Components.Add(component);

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);
            model.DependencyUsages.Add("usage", "");

            FormatException ex = Assert.Throws<FormatException>(() => platformDependency.Validate(model));
            Assert.Equal(
                $"Platform dependency with type '{DependencyType.Executable}' and ID 'name' overrides a dependency with type " +
                $"'{DependencyType.Executable}' and ID 'bad_ref' that doesn't exist in its platform hierarchy.",
                ex.Message);
        }

        internal static PlatformDependency CreatePlatformDependencyWithoutUsage() =>
            new("test", DependencyType.LinuxPackage, usage: null, id: null);

        internal static void ValidateUsageNotSetException(Action action)
        {
            FormatException ex = Assert.Throws<FormatException>(action);

            Assert.Equal(
                "Usage must be set for platform dependencies that do not have an override set.",
                ex.Message);
        }
    }
}
