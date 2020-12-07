// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class ProductCollectionTests : TestBase
    {
        [Fact]
        public async Task ItReturnsAllSupportPhases()
        {
            var products = await ProductCollection.GetFromFileAsync(@"data\\releases-index.json", false);
            var supportPhases = products.GetSupportPhases();
            
            Assert.Equal(3, supportPhases.Count());
            Assert.Contains(SupportPhase.EOL, supportPhases);
            Assert.Contains(SupportPhase.LTS, supportPhases);
            Assert.Contains(SupportPhase.Preview, supportPhases);
        }
    }
}
