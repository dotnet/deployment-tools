// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class PlatformTests
    {
        [Fact]
        public void ItSetsInitialState()
        {
            Platform platform = new("alpine");
            Assert.Equal("alpine", platform.Rid);
            Assert.Empty(platform.Components);
            Assert.Empty(platform.Platforms);
        }

        [Fact]
        public void ItSucceedsValidationWhenComponentsHaveSameNameButDifferentType()
        {
            Platform platform = new("alpine");
            platform.Components.Add(new Component("component", ComponentType.SharedFramework));
            platform.Components.Add(new Component("component", ComponentType.NuGetPackage));

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);

            platform.Validate(model);
        }

        [Fact]
        public void ItThrowsWhenWhenDuplicateComponentsAreFound()
        {
            Platform platform = CreatePlatformWithDuplicateComponents("alpine");

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);

            ValidateDuplicateComponentsException("alpine", () => platform.Validate(model));
        }

        [Fact]
        public void ItThrowsIfRidIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new Platform(null!));
            ArgumentException ex = Assert.Throws<ArgumentException>(() => new Platform(string.Empty));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: rid", ex.Message);
        }

        [Fact]
        public void ItValidatesComponents()
        {
            Component component = ComponentTests.CreateComponentWithUsageNotSetInPlatformDependency();

            Platform platform = new("debian");
            platform.Components.Add(component);

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(platform);

            PlatformDependencyTests.ValidateUsageNotSetException(() => platform.Validate(model));
        }

        [Fact]
        public void ItValidatesChildPlatforms()
        {
            Platform rootPlatform = new("debian");
            Platform childPlatform = CreatePlatformWithDuplicateComponents("debian.11");
            rootPlatform.Platforms.Add(childPlatform);

            PlatformDependenciesModel model = new(new Version("1.0"));
            model.Platforms.Add(rootPlatform);

            ValidateDuplicateComponentsException("debian.11", () => rootPlatform.Validate(model));
        }

        private static Platform CreatePlatformWithDuplicateComponents(string rid)
        {
            Platform platform = new(rid);
            platform.Components.Add(new Component("component", ComponentType.SharedFramework));
            platform.Components.Add(new Component("component", ComponentType.SharedFramework));
            return platform;
        }

        private static void ValidateDuplicateComponentsException(string rid, Action action)
        {
            FormatException ex = Assert.Throws<FormatException>(action);

            Assert.Equal(
                $"Detected duplicate components in platform '{rid}'. Each component's name and type should represent a " +
                "unique identity within a platform.",
                ex.Message);
        }
    }
}
