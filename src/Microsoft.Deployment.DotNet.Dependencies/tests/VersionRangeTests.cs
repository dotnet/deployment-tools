// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class VersionRangeTests
    {
        [Theory]
        [InlineData("1.0", "1.0", true, null, false)]         // x >= 1.0
        [InlineData("[1.0,)", "1.0", true, null, false)]      // x >= 1.0
        [InlineData("(1.0,)", "1.0", false, null, false)]     // x > 1.0
        [InlineData("[1.0]", "1.0", true, "1.0", true)]       // x == 1.0
        [InlineData("(,1.0]", null, false, "1.0", true)]      // x <= 1.0
        [InlineData("(,1.0)", null, false, "1.0", false)]     // x < 1.0
        [InlineData("[1.0,2.0]", "1.0", true, "2.0", true)]   // 1.0 <= x <= 2.0
        [InlineData("(1.0,2.0)", "1.0", false, "2.0", false)] // 1.0 < x < 2.0
        [InlineData("[1.0,2.0)", "1.0", true, "2.0", false)]  // 1.0 <= x < 2.0
        public void ItParses(string input, string? expectedMin, bool expectedIsMinimumInclusive,
            string? expectedMax, bool expectedIsMaximumInclusive)
        {
            VersionRange result = VersionRange.Parse(input);
            ValidateVersionRange(result, expectedMin, expectedIsMinimumInclusive, expectedMax, expectedIsMaximumInclusive);
        }

        [Fact]
        public void ItThrowsIfParseInputIsEmpty()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => VersionRange.Parse(string.Empty));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: versionRange", ex.Message);
        }

        [Fact]
        public void ItThrowsIfTooManyVersionsExist()
        {
            string range = "(1.0,2.0,3.0)";
            ArgumentException ex = Assert.Throws<ArgumentException>(() => VersionRange.Parse(range));
            Assert.Equal($"Version range '{range}' is invalid. A maximum of 2 version numbers are allowed in the range.{Environment.NewLine}Parameter name: versionRange", ex.Message);
        }

        [Fact]
        public void ItThrowsIfMaxIsLessThanMin()
        {
            string range = "(2.2,2.1)";
            ArgumentException ex = Assert.Throws<ArgumentException>(() => VersionRange.Parse(range));
            Assert.Equal($"Version range '{range}' is invalid. The minimum version must be less than the maximum version.{ Environment.NewLine}Parameter name: versionRange", ex.Message);
        }

        [Fact]
        public void ItThrowsIfExclusiveRangeResultsInNoMatches()
        {
            string range = "(1.0)";
            ArgumentException ex = Assert.Throws<ArgumentException>(() => VersionRange.Parse(range));
            Assert.Equal($"Version range '{range}' is invalid. The exclusive notation used for both minimum and maximum causes no possible version matches.{ Environment.NewLine}Parameter name: versionRange", ex.Message);
        }

        internal static void ValidateVersionRange(VersionRange? versionRange, string? expectedMin,
            bool expectedIsMinimumInclusive, string? expectedMax, bool expectedIsMaximumInclusive)
        {
            Assert.Equal(expectedMin, versionRange?.Minimum?.ToString());
            Assert.Equal(expectedIsMinimumInclusive, versionRange?.IsMinimumInclusive ?? false);
            Assert.Equal(expectedMax, versionRange?.Maximum?.ToString());
            Assert.Equal(expectedIsMaximumInclusive, versionRange?.IsMaximumInclusive ?? false);
        }
    }
}
