// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Deployment.DotNet.Dependencies.Tests
{
    public class ExpressionTests
    {
        [Theory]
        [MemberData(nameof(ParseTestInput))]
        public void ItCanParse(ParseTestData testData)
        {
            NameExpression result = NameExpression.Parse(testData.Input);
            testData.Validate(result);
        }

        [Fact]
        public void ItThrowsIfParseInputIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => NameExpression.Parse(null!));
        }

        [Fact]
        public void ItThrowsIfParseInputIsEmpty()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => NameExpression.Parse(string.Empty));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: expression", ex.Message);
        }

        private static IEnumerable<object[]> ParseTestInput()
        {
            ParseTestData[] testDataObjs = new ParseTestData[]
            {
                new ParseTestData("dep1", result =>
                {
                    Assert.Single(result.Names);

                    DependencyName depName = result.Names[0];
                    Assert.Equal("dep1", depName.Name);
                    Assert.Null(depName.VersionRange);
                }),

                new ParseTestData("dep1:1.0", result =>
                {
                    Assert.Single(result.Names);

                    DependencyName depName = result.Names[0];
                    Assert.Equal("dep1", depName.Name);
                    VersionRangeTests.ValidateVersionRange(
                        depName.VersionRange,
                        expectedMin: "1.0", expectedIsMinimumInclusive: true,
                        expectedMax: null, expectedIsMaximumInclusive: false);
                }),

                new ParseTestData("dep1:[2.0,3.0) || dep2:1.1", result =>
                {
                    Assert.Equal(2, result.Names.Count);

                    DependencyName dep1 = result.Names[0];

                    Assert.Equal("dep1", dep1.Name);
                    VersionRangeTests.ValidateVersionRange(
                        dep1.VersionRange,
                        expectedMin: "2.0", expectedIsMinimumInclusive: true,
                        expectedMax: "3.0", expectedIsMaximumInclusive: false);

                    DependencyName dep2 = result.Names[1];

                    Assert.Equal("dep2", dep2.Name);
                    VersionRangeTests.ValidateVersionRange(
                        dep2.VersionRange,
                        expectedMin: "1.1", expectedIsMinimumInclusive: true,
                        expectedMax: null, expectedIsMaximumInclusive: false);
                }),

                new ParseTestData("dep1 || dep2 || dep3", result =>
                {
                    Assert.Equal(3, result.Names.Count);

                    DependencyName dep1 = result.Names[0];
                    Assert.Equal("dep1", dep1.Name);
                    Assert.Null(dep1.VersionRange);

                    DependencyName dep2 = result.Names[1];
                    Assert.Equal("dep2", dep2.Name);
                    Assert.Null(dep2.VersionRange);

                    DependencyName dep3 = result.Names[2];
                    Assert.Equal("dep3", dep3.Name);
                    Assert.Null(dep3.VersionRange);
                })
            };

            return testDataObjs.Select(data => new object[] { data });
        }

        public class ParseTestData
        {
            public string Input { get; }

            public Action<NameExpression> Validate { get; }

            public ParseTestData(string input, Action<NameExpression> validate)
            {
                Input = input;
                Validate = validate;
            }
        }
    }
}
