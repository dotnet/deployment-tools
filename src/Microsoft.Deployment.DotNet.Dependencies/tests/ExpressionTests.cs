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
            Expression result = Expression.Parse(testData.Input);
            testData.Validate(result);
        }

        [Fact]
        public void ItThrowsIfParseInputIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Expression.Parse(null!));
        }

        [Fact]
        public void ItThrowsIfParseInputIsEmpty()
        {
            ArgumentException ex = Assert.Throws<ArgumentException>(() => Expression.Parse(string.Empty));
            Assert.Equal($"Value cannot be empty.{Environment.NewLine}Parameter name: expression", ex.Message);
        }

        private static IEnumerable<object[]> ParseTestInput()
        {
            ParseTestData[] testDataObjs = new ParseTestData[]
            {
                new ParseTestData("dep1", result =>
                {
                    Assert.IsType<DependencyName>(result);

                    DependencyName depName = (DependencyName)result;
                    Assert.Equal("dep1", depName.Name);
                    Assert.Null(depName.VersionRange);
                }),

                new ParseTestData("dep1:1.0", result =>
                {
                    Assert.IsType<DependencyName>(result);

                    DependencyName depName = (DependencyName)result;
                    Assert.Equal("dep1", depName.Name);
                    VersionRangeTests.ValidateVersionRange(
                        depName.VersionRange,
                        expectedMin: "1.0", expectedIsMinimumInclusive: true,
                        expectedMax: null, expectedIsMaximumInclusive: false);
                }),

                new ParseTestData("dep1:[2.0,3.0) || dep2:1.1", result =>
                {
                    Assert.IsType<BinaryExpression>(result);

                    BinaryExpression binExpr = (BinaryExpression)result;

                    Assert.IsType<DependencyName>(binExpr.Left);
                    DependencyName leftDep = (DependencyName)binExpr.Left;

                    Assert.Equal("dep1", leftDep.Name);
                    VersionRangeTests.ValidateVersionRange(
                        leftDep.VersionRange,
                        expectedMin: "2.0", expectedIsMinimumInclusive: true,
                        expectedMax: "3.0", expectedIsMaximumInclusive: false);

                    Assert.IsType<DependencyName>(binExpr.Right);
                    DependencyName rightDep = (DependencyName)binExpr.Right;

                    Assert.Equal("dep2", rightDep.Name);
                    VersionRangeTests.ValidateVersionRange(
                        rightDep.VersionRange,
                        expectedMin: "1.1", expectedIsMinimumInclusive: true,
                        expectedMax: null, expectedIsMaximumInclusive: false);
                }),

                new ParseTestData("dep1 || dep2 || dep3", result =>
                {
                    Assert.IsType<BinaryExpression>(result);

                    BinaryExpression binExpr = (BinaryExpression)result;

                    Assert.IsType<BinaryExpression>(binExpr.Left);
                    BinaryExpression leftBinExpr = (BinaryExpression)binExpr.Left;

                    Assert.IsType<DependencyName>(leftBinExpr.Left);
                    DependencyName dep1 = (DependencyName)leftBinExpr.Left;

                    Assert.Equal("dep1", dep1.Name);
                    Assert.Null(dep1.VersionRange);

                    Assert.IsType<DependencyName>(leftBinExpr.Right);
                    DependencyName dep2 = (DependencyName)leftBinExpr.Right;

                    Assert.Equal("dep2", dep2.Name);
                    Assert.Null(dep2.VersionRange);

                    Assert.IsType<DependencyName>(binExpr.Right);
                    DependencyName dep3 = (DependencyName)binExpr.Right;

                    Assert.Equal("dep3", dep3.Name);
                    Assert.Null(dep3.VersionRange);
                })
            };

            return testDataObjs.Select(data => new object[] { data });
        }

        public class ParseTestData
        {
            public string Input { get; }

            public Action<Expression> Validate { get; }

            public ParseTestData(string input, Action<Expression> validate)
            {
                Input = input;
                Validate = validate;
            }
        }
    }
}
