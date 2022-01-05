// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Deployment.DotNet.Dependencies
{
    /// <summary>
    /// A dependency name represented as an expression.
    /// </summary>
    public class NameExpression
    {
        /// <summary>
        /// The set of names that represent the dependency, only one of which needs to apply in order to meet the dependency condition.
        /// </summary>
        /// <remarks>
        /// Logically, each item in the set is OR'd together.
        /// </remarks>
        public IList<DependencyName> Names { get; } = new List<DependencyName>();

        /// <summary>
        /// Parses the expression into its component parts.
        /// </summary>
        /// <param name="expression">String to be parsed as an expression.</param>
        /// <returns>A dependency name expression representing the component parts of the expression.</returns>
        public static NameExpression Parse(string expression)
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

            NameExpression nameExpression = new();

            for (int i = 0; i < operands.Length; i++)
            {
                DependencyName name = DependencyName.Parse(operands[i].Trim());
                nameExpression.Names.Add(name);
                
            }

            return nameExpression;
        }
    }
}
