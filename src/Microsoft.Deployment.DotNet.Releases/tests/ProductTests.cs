// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class ProductsTests : TestBase
    {
        private const string s_productWithEolSupportPhaseJson = @"{""channel-version"": ""2.2"", ""latest-release"": ""2.2.8"", ""latest-release-date"": ""2019-11-19"",
""security"": true, ""latest-runtime"": ""2.2.8"", ""latest-sdk"": ""2.2.207"", ""product"": "".NET Core"", ""support-phase"": ""eol"",
""eol-date"": ""2019-12-23"", ""releases.json"": ""https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/2.2/releases.json""}";

        private const string s_productWithEolDateJson = @"{""channel-version"": ""2.2"", ""latest-release"": ""2.2.8"", ""latest-release-date"": ""2019-11-19"",
""security"": true, ""latest-runtime"": ""2.2.8"", ""latest-sdk"": ""2.2.207"", ""product"": "".NET Core"", ""support-phase"": ""current"",
""eol-date"": ""2019-12-23"", ""releases.json"": ""https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/2.2/releases.json""}";

        private const string s_productWithNullEolDateJson = @"{""channel-version"": ""2.2"", ""latest-release"": ""2.2.8"", ""latest-release-date"": ""2019-11-19"",
""security"": true, ""latest-runtime"": ""2.2.8"", ""latest-sdk"": ""2.2.207"", ""product"": "".NET Core"", ""support-phase"": ""current"",
""eol-date"": null, ""releases.json"": ""https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/2.2/releases.json""}";

        [Fact]
        public void IsOutOfSupportChecksEolDateIfSupportPhaseIsNotEol()
        {
            Product product = CreateProduct(s_productWithEolDateJson);

            Assert.True(product.IsOutOfSupport());
        }

        [Fact]
        public void IsOutOfSupportChecksSupportPhaseFirst()
        {
            Product product = CreateProduct(s_productWithEolSupportPhaseJson);

            Assert.True(product.IsOutOfSupport());
        }

        [Fact]
        public void IsOutOfSupportReturnsFalseIfEolDateIsNull()
        {
            Product product = CreateProduct(s_productWithNullEolDateJson);

            Assert.False(product.IsOutOfSupport());
        }

        [Fact]
        public void Properties()
        {
            var product = Products.Where(p => p.ProductVersion == "3.1").FirstOrDefault();

            Assert.Equal(".NET Core", product.ProductName);
            Assert.Equal("2022-10-11", product.LatestReleaseDate.ToString("yyyy-MM-dd"));
            Assert.True(product.LatestReleaseIncludesSecurityUpdate);
        }
    }
}
