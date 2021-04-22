// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class DependencyNameTests
    {
        [Theory]
        [InlineData("dep1", "dep1", null, false, null, false)]
        [InlineData("dep1:1.0", "dep1", "1.0", true, null, false)]
        [InlineData("dep1:[1.0,2.0)", "dep1", "1.0", true, "2.0", false)]
        public void ItCanParse(string input, string expectedName, string? expectedMin, bool expectedIsMinimumInclusive,
            string? expectedMax, bool expectedIsMaximumInclusive)
        {
            DependencyName result = DependencyName.Parse(input);

            Assert.Equal(expectedName, result.Name);
            VersionRangeTests.ValidateVersionRange(
                result.VersionRange, expectedMin, expectedIsMinimumInclusive, expectedMax, expectedIsMaximumInclusive);
        }

        [Fact]
        public void ItThrowsIfParseInputIsEmpty()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => DependencyName.Parse(string.Empty));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: dependencyName", ex.Message);
        }
    }
}
