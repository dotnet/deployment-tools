// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// A type of expression node.
    /// </summary>
    public abstract class Expression
    {
        /// <summary>
        /// Gets the type of expression.
        /// </summary>
        public abstract ExpressionType NodeType { get; }

        /// <summary>
        /// Parses the expression into its component parts.
        /// </summary>
        /// <param name="expression">String to be parsed as an expression.</param>
        /// <returns>An expression node representing the component parts of the expression.</returns>
        public static Expression Parse(string expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(expression));
            }

            string[] operands = expression.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            Expression? current = null;

            for (int i = 0; i < operands.Length; i++)
            {
                DependencyName name = DependencyName.Parse(operands[i].Trim());
                if (current is null)
                {
                    current = name;
                }
                else
                {
                    current = new BinaryExpression(current, name);
                }
            }

            if (current is null)
            {
                throw new ArgumentException(
                    $"Dependency name expression '{expression}' is not a valid expression.",
                    nameof(expression));
            }

            return current;
        }
    }

    /// <summary>
    /// The type of an expression node.
    /// </summary>
    public enum ExpressionType
    {
        /// <summary>
        /// Indicates the expression node represents a dependency name.
        /// </summary>
        DependencyName,

        /// <summary>
        /// Indicates the expression node represents a Boolean OR operator.
        /// </summary>
        Or
    }

    /// <summary>
    /// Represents a binary expression with two child expression nodes.
    /// </summary>
    public class BinaryExpression : Expression
    {
        /// <summary>
        /// Gets the type of expression.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Or;

        /// <summary>
        /// Gets the left operand of the binary expression.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// Gets the right operand of the binary expression.
        /// </summary>
        public Expression Right { get; }

        /// <summary>
        /// Initializes an instance of <see cref="BinaryExpression"/>.
        /// </summary>
        /// <param name="left">Left operand of the binary expression.</param>
        /// <param name="right">Right operand of the binary expression.</param>
        public BinaryExpression(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }
    }

    /// <summary>
    /// Represents a reference to a dependency name and optional version information.
    /// </summary>
    public class DependencyName : Expression
    {
        /// <summary>
        /// Gets the type of expression.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.DependencyName;

        /// <summary>
        /// Gets the name of the dependency.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the version range information for the dependency.
        /// </summary>
        public VersionRange? VersionRange { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DependencyName"/>.
        /// </summary>
        /// <param name="name">Name of the dependency.</param>
        public DependencyName(string name)
        {
            Name = name;
        }

        internal static new DependencyName Parse(string dependencyName)
        {
            if (dependencyName == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(dependencyName));
            }

            int versionRangeSeparatorIndex = dependencyName.IndexOf(':');
            if (versionRangeSeparatorIndex > 0)
            {
                return new DependencyName(dependencyName.Substring(0, versionRangeSeparatorIndex))
                {
                    VersionRange = VersionRange.Parse(dependencyName.Substring(versionRangeSeparatorIndex + 1))
                };
            }
            else
            {
                return new DependencyName(dependencyName);
            }
        }
    }

    /// <summary>
    /// Represents the version range of a dependency.
    /// </summary>
    public class VersionRange
    {
        private const char MinExclusiveChar = '(';
        private const char MaxExclusiveChar = ')';
        private const char MinInclusiveChar = '[';
        private const char MaxInclusiveChar = ']';

        /// <summary>
        /// Gets the minimum version of the dependency.
        /// </summary>
        public Version? Minimum { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Minimum"/> is inclusive within the range.
        /// </summary>
        public bool IsMinimumInclusive { get; private set; }

        /// <summary>
        /// Gets the maximum version of the dependency.
        /// </summary>
        public Version? Maximum { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Maximum"/> is inclusive within the range.
        /// </summary>
        public bool IsMaximumInclusive { get; private set; }

        internal static VersionRange Parse(string versionRange)
        {
            if (versionRange == string.Empty)
            {
                throw new ArgumentException("Value cannot be empty.", nameof(versionRange));
            }

            bool isMinimumInclusive = versionRange[0] == MinInclusiveChar || versionRange[0] != MinExclusiveChar;
            bool isMaximumInclusive = versionRange.Last() == MaxInclusiveChar;

            // Get the version numbers from within the range notation
            string[] minMax = versionRange
                .TrimStart(MinExclusiveChar, MinInclusiveChar)
                .TrimEnd(MaxExclusiveChar, MaxInclusiveChar)
                .Split(',');

            Version? minimum = null;
            Version? maximum = null;

            if (minMax.Length > 1)
            {
                if (minMax.Length > 2)
                {
                    throw new ArgumentException(
                        $"Version range '{versionRange}' is invalid. A maximum of 2 version numbers are allowed in the range.",
                        nameof(versionRange));
                }

                if (minMax[0].Length > 0)
                {
                    minimum = Version.Parse(minMax[0]);
                }

                if (minMax[1].Length > 0)
                {
                    maximum = Version.Parse(minMax[1]);
                }
            }
            else
            {
                minimum = Version.Parse(minMax[0]);
                if (minMax.Length == 1 && isMaximumInclusive)
                {
                    maximum = minimum;
                }
            }

            if (minimum is not null && maximum is not null && minimum > maximum)
            {
                throw new ArgumentException(
                    $"Version range '{versionRange}' is invalid. The minimum version must be less than the maximum version.",
                    nameof(versionRange));
            }

            if (!isMinimumInclusive && maximum is null && minMax.Length == 1)
            {
                // Example: (1.0)
                throw new ArgumentException(
                    $"Version range '{versionRange}' is invalid. The exclusive notation used for both minimum and maximum causes no possible version matches.",
                    nameof(versionRange));
            }

            return new VersionRange
            {
                Minimum = minimum,
                IsMinimumInclusive = isMinimumInclusive,
                Maximum = maximum,
                IsMaximumInclusive = isMaximumInclusive
            };
        }
    }
}
