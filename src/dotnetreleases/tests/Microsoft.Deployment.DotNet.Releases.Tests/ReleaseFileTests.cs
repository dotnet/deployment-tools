// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class ReleaseFileTests
    {
        [Fact]
        public void ItReturnsTrueIfReferenceEquals()
        {
            var f1 = new ReleaseFile("abcdef", "foo", "win-x86", "https://here.there.com");
            var f2 = f1;

            Assert.Equal(f1, f2);
        }

        [Fact]
        public void ItReturnsTrueIfReferenceEquals2()
        {
            var f1 = new ReleaseFile("abcdef", "foo", "win-x86", "https://here.there.com");
            var f2 = new ReleaseFile("abcdef", "foo", "win-x86", "https://here.there.com");

            Assert.Equal(f1, f2);
        }

        [Fact]
        public void ItReturnsFalseIfReferenceEquals2()
        {
            var f1 = new ReleaseFile("abcdef", "Foo", "win-x86", "https://here.there.com");
            var f2 = new ReleaseFile("abcdef", "foo", "win-x86", "https://here.there.com");

            Assert.NotEqual(f1, f2);
        }

        [Fact]
        public void ItComputesTheFileNameFromTheUrl()
        {
            var f1 = new ReleaseFile("abcdef", "Foo", "win-x86", "https://here.there.com/foo-win-x86.exe");

            Assert.Equal("foo-win-x86.exe", f1.FileName);
        }
    }
}
