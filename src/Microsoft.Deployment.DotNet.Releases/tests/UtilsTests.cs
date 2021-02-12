// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using Xunit;

namespace Microsoft.Deployment.DotNet.Releases.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void GetFileHashThrowsIfFileNameIsNull()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(() =>
            {
                Utils.GetFileHash(null, SHA512Managed.Create());
            });
        }

        [Fact]
        public void GetFileHashThrowsIfFileNameIsEmpty()
        {
            ArgumentException e = Assert.Throws<ArgumentException>(() =>
            {
                Utils.GetFileHash("", SHA512Managed.Create());
            });

            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: fileName", e.Message);
        }

        [Fact]
        public void GetFileHashThrowsIfHashAlgorithmIsNull()
        {
            ArgumentNullException e = Assert.Throws<ArgumentNullException>(() =>
            {
                Utils.GetFileHash("File.txt", null);
            });

            Assert.Equal($"Value cannot be null.{Environment.NewLine}Parameter name: hashAlgorithm", e.Message);
        }
    }
}
