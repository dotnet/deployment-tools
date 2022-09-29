// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class SupportPhaseConverterTests : TestBase
    {
        public class TestObject
        {
            public SupportPhase SupportPhase
            {
                get;
                set;
            }
        }

        [Theory]
        [InlineData("eoL", SupportPhase.EOL)]
        [InlineData("LTS", SupportPhase.LTS)]
        [InlineData("current", SupportPhase.Current)]
        [InlineData("rC", SupportPhase.RC)]
        public void ItIsCaseInsenitive(string supportPhaseValue, SupportPhase expectedSupportPhase)
        {
            var json = $@"{{""SupportPhase"":""{supportPhaseValue}""}}";
            var testObject = JsonSerializer.Deserialize<TestObject>(json, SerializerOptions.Default);

            Assert.Equal(expectedSupportPhase, testObject.SupportPhase);
        }

        [Theory]
        [InlineData("X63")]
        [InlineData("")]
        public void ItReturnsUnknownIfParsingFails(string supportPhaseValue)
        {
            var json = $@"{{""SupportPhase"":""{supportPhaseValue}""}}";
            var testObject = JsonSerializer.Deserialize<TestObject>(json, SerializerOptions.Default);

            Assert.Equal(SupportPhase.Unknown, testObject.SupportPhase);
        }
    }
}
