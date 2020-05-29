#region Copyright (c) OPTANO GmbH

// ////////////////////////////////////////////////////////////////////////////////
// 
//        OPTANO GmbH Source Code
//        Copyright (c) 2010-2020 OPTANO GmbH
//        ALL RIGHTS RESERVED.
// 
//    The entire contents of this file is protected by German and
//    International Copyright Laws. Unauthorized reproduction,
//    reverse-engineering, and distribution of all or any portion of
//    the code contained in this file is strictly prohibited and may
//    result in severe civil and criminal penalties and will be
//    prosecuted to the maximum extent possible under the law.
// 
//    RESTRICTIONS
// 
//    THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES
//    ARE CONFIDENTIAL AND PROPRIETARY TRADE SECRETS OF
//    OPTANO GMBH.
// 
//    THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED
//    FILES OR ANY PORTION OF ITS CONTENTS SHALL AT NO TIME BE
//    COPIED, TRANSFERRED, SOLD, DISTRIBUTED, OR OTHERWISE MADE
//    AVAILABLE TO OTHER INDIVIDUALS WITHOUT WRITTEN CONSENT
//    AND PERMISSION FROM OPTANO GMBH.
// 
// ////////////////////////////////////////////////////////////////////////////////

#endregion

namespace Optano.Algorithm.Tuner.Saps.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    using Shouldly;

    /// <summary>
    /// Contains utility methods that are helpful in tests.
    /// </summary>
    public static class TestUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// Prints a list in the form { item1, item2, item3, item4 }.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="list">The list to print.</param>
        /// <returns>A <see cref="string"/> representing the given list.</returns>
        public static string PrintList<T>(IEnumerable<T> list)
        {
            return $"{{{string.Join(", ", list)}}}";
        }

        /// <summary>
        /// Checks console output on invoking a certain action.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="check">Checks to do on the output.</param>
        public static void CheckOutput(Action action, Action<StringWriter> check)
        {
            using StringWriter consoleOutput = new StringWriter();
            // Redirect console output.
            var originalOut = Console.Out;
            Console.SetOut(consoleOutput);

            // Execute action.
            action.Invoke();

            try
            {
                // Execute check.
                check.Invoke(consoleOutput);
            }
            finally
            {
                // Don't forget to redo the output redirect.
                Console.SetOut(originalOut);
            }
        }

        /// <summary>
        /// Returns whether the given <see cref="IParameterTreeNode"/> represents the parameter described by the given
        /// identifier.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="identifier">The identifier to check for.</param>
        /// <returns>True if and only if the node is an <see cref="IParameterNode"/> with identifier equal to the given
        /// one.
        /// </returns>
        public static bool RepresentsParameter(IParameterTreeNode node, string identifier)
        {
            return node is IParameterNode parameterNode && parameterNode.Identifier == identifier;
        }

        /// <summary>
        /// Asserts that the domain has the expected range.
        /// </summary>
        /// <param name="domain">Domain to check.</param>
        /// <param name="identifier">Parameter's identifier. Useful for output.</param>
        /// <param name="expectedMinimum">The expected minimum.</param>
        /// <param name="expectedMaximum">The expected maximum.</param>
        public static void CheckRange(
            NumericalDomain<double> domain,
            string identifier,
            double expectedMinimum,
            double expectedMaximum)
        {
            expectedMinimum.ShouldBe(domain.Minimum, $"Minimum value for {identifier} should be different.");
            expectedMaximum.ShouldBe(domain.Maximum, $"Maximum value for {identifier} should be different.");
        }

        #endregion
    }
}