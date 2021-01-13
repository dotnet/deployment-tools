// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class ProductReleaseTests : TestBase
    {
        [Theory]
        [InlineData("2.2", "2.2.8", "2.2.110", "2.2.207")]
        [InlineData("2.1", "2.1.20", "2.1.808", "2.1.613", "2.1.516")]
        [InlineData("2.0", "2.1.201", "2.1.201")]
        [InlineData("1.1", "1.1.13", "1.1.14")]
        public void ItContainsAllSdks(string productVersion, string releaseVersion, params string[] expectedSdkVersions)
        {
            var productReleaseVersion = new ReleaseVersion(releaseVersion);
            var release = ProductReleases[productVersion].Where(r => r.Version == productReleaseVersion).FirstOrDefault();
            var actualSdkVersions = release.Sdks.Select(s => s.Version.ToString());

            Assert.Equal(expectedSdkVersions.Length, release.Sdks.Count);
            Assert.All(expectedSdkVersions, sdkVersion => actualSdkVersions.Contains(sdkVersion));
        }

        [Theory]
        [InlineData("2.0", "2.1.201")]
        [InlineData("1.1", "1.1.13")]
        public void ItLinksBackToProduct(string productVersion, string releaseVersion)
        {
            var productReleaseVersion = new ReleaseVersion(releaseVersion);
            var release = ProductReleases[productVersion].Where(r => r.Version == productReleaseVersion).FirstOrDefault();

            Assert.Equal(release.Product.ProductVersion, productVersion);
        }
    }
}
