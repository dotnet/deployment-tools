// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class DependencyRefTests
    {
        [Fact]
        public void ItSetsInitialState()
        {
            DependencyRef depRef = new("depName", DependencyType.LinuxPackage);
            Assert.Equal("depName", depRef.Id);
        }

        [Fact]
        public void ItThrowsIfIdIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new DependencyRef(null!, DependencyType.LinuxPackage));
            ArgumentException ex = Assert.Throws<ArgumentException>(() => new DependencyRef(string.Empty, DependencyType.LinuxPackage));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: id", ex.Message);
        }

        [Theory]
        [InlineData("dep", DependencyType.LinuxPackage, "dep", DependencyType.LinuxPackage, true)]
        [InlineData("dep", DependencyType.LinuxPackage, "dep", DependencyType.Executable, false)]
        [InlineData("dep", DependencyType.LinuxPackage, "dep2", DependencyType.LinuxPackage, false)]
        public void ItIsReferenceToDependency(string refId, DependencyType refType,
            string platformDependencyName, DependencyType platformDependencyType, bool areEqual)
        {
            DependencyRef depRef = new(refId, refType);
            PlatformDependency platformDependency = new(platformDependencyName, platformDependencyType, usage: "default");

            Assert.Equal(areEqual, depRef.IsReferenceTo(platformDependency));
        }
    }
}
