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

namespace Optano.Algorithm.Tuner.Application.Tests
{
    using System;
    using System.IO;

    using NLog;
    using NLog.Config;
    using NLog.Targets;

    using Xunit;

    /// <summary>
    /// Contains utility methods that are helpful in tests.
    /// </summary>
    public static class TestUtils
    {
        #region Constants

        /// <summary>
        /// The path to a CPU intensive target algorithm. We recommend to use the ubcsat implementation of the SAT solver SAPS.
        /// </summary>
        public const string PathToTargetAlgorithm = @"Tools/ubcsat.exe";

        /// <summary>
        /// The path to a test instance of the CPU intensive target algorithm. 
        /// </summary>
        public const string PathToTestInstance = @"Tools/testInstance.cnf";

        /// <summary>
        /// The path to the test application, which is used in multiple tests.
        /// </summary>
        private const string PathToTestApplication = @"Tools/TestApplication.dll";

        #endregion

        #region Static Fields

        /// <summary>
        /// Call of the application, which returns its input as exit code.
        /// </summary>
        public static readonly string ReturnExitCodeApplicationCall = $"dotnet {TestUtils.PathToTestApplication} returnExitCode";

        /// <summary>
        /// Call of the application, which returns its input.
        /// </summary>
        public static readonly string ReturnInputApplicationCall = $"dotnet {TestUtils.PathToTestApplication} returnInput";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Checks output on invoking a certain action.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="check">Checks to do on the output.</param>
        public static void CheckOutput(Action action, Action<string> check)
        {
            // grab output to console.
            var originalOut = Console.Out;
            var writer = new StringWriter();

            try
            {
                Console.SetOut(writer);
                action.Invoke();
                check.Invoke(writer.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        /// <summary>
        /// Initializes the <see cref="NLog"/> logger with a config that is suitable for tests.
        /// </summary>
        public static void InitializeLogger()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Checks that <paramref name="value"/> equals <paramref name="expected"/> within a certain tolerance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="additionalInformation">The additional information to write on failure.</param>
        public static void Equals(double value, double expected, double tolerance, string additionalInformation = null)
        {
            var message = $"{value} does not equal {expected} with tolerance {tolerance}";
            if (additionalInformation != null)
            {
                message += $"\n{additionalInformation}";
            }

            Assert.True(Math.Abs(value - expected) < tolerance, message);
        }

        #endregion
    }
}