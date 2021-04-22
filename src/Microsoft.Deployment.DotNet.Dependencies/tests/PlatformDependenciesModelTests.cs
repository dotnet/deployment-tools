// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class PlatformDependenciesModelTests
    {
        [Fact]
        public void ItSetsInitialState()
        {
            PlatformDependenciesModel model = new(new Version("1.0"));
            Assert.Equal("1.0", model.ProductVersion.ToString());
            Assert.Empty(model.DependencyUsages);
            Assert.Empty(model.Platforms);
        }

        [Fact]
        public async void ItLoadsModelFromFile()
        {
            PlatformDependenciesModel model = await PlatformDependenciesModel.GetFromFileAsync("data//basic.json");
            Assert.Single(model.DependencyUsages);
            Assert.Equal("6.0", model.ProductVersion.ToString());
            Assert.Single(model.Platforms);
        }

        [Fact]
        public async void ItThrowsWhenLoadingEmptyFile()
        {
            const string path = "data//empty-file.json";
            FormatException ex = await Assert.ThrowsAsync<FormatException>(() => PlatformDependenciesModel.GetFromFileAsync(path));
            Assert.Equal($"Unable to deserialize the content in file '{path}'.", ex.Message);
        }

        [Fact]
        public async void ItThrowsWhenModelContainsDuplicatePlatforms()
        {
            FormatException ex = await Assert.ThrowsAsync<FormatException>(
                () => PlatformDependenciesModel.GetFromFileAsync("data//duplicate-platforms.json"));
            Assert.Equal("Duplicate platforms were found with RID 'debian'.", ex.Message);
        }
    }
}
