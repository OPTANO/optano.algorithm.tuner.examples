#region Copyright (c) OPTANO GmbH

// ////////////////////////////////////////////////////////////////////////////////
// 
//        OPTANO GmbH Source Code
//        Copyright (c) 2010-2021 OPTANO GmbH
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

namespace Optano.Algorithm.Tuner.Gurobi.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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

        #endregion
    }
}