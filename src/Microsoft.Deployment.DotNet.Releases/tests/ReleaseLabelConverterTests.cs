// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class ReleaseLabelConverterTests : TestBase
    {
        public class TestObject
        {
            public ReleaseLabel SupportPhase
            {
                get;
                set;
            }
        }

        [Theory]
        [InlineData("eoL", ReleaseLabel.EOL)]
        [InlineData("LTS", ReleaseLabel.LTS)]
        [InlineData("sts", ReleaseLabel.STS)]
        [InlineData("go-live", ReleaseLabel.GoLive)]
        public void ItIsCaseInsenitive(string supportPhaseValue, ReleaseLabel expectedSupportPhase)
        {
            var json = $@"{{""SupportPhase"":""{supportPhaseValue}""}}";
            var testObject = JsonConvert.DeserializeObject<TestObject>(json, new ReleaseLabelConverter());

            Assert.Equal(expectedSupportPhase, testObject.SupportPhase);
        }

        [Theory]
        [InlineData("X63")]
        [InlineData("")]
        public void ItReturnsUnknownIfParsingFails(string supportPhaseValue)
        {
            var json = $@"{{""SupportPhase"":""{supportPhaseValue}""}}";
            var testObject = JsonConvert.DeserializeObject<TestObject>(json, new ReleaseLabelConverter());

            Assert.Equal(ReleaseLabel.Unknown, testObject.SupportPhase);
        }
    }
}
