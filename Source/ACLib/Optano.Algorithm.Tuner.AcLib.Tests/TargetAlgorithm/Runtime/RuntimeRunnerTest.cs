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

namespace Optano.Algorithm.Tuner.AcLib.Tests.TargetAlgorithm.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Runtime;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="RuntimeRunner"/> class.
    /// </summary>
    public class RuntimeRunnerTest : RunnerBaseTest<RuntimeResult>
    {
        #region Public Methods and Operators

        /// <summary>
        /// Checks that <see cref="RuntimeRunner"/> sets results with status TIMEOUT to cancelled and assigns them
        /// the maximum runtime.
        /// </summary>
        [Fact]
        public void TimedOutResultsHaveMaximumRuntime()
        {
            var scenario = this.BuildResultEchoScenario("TIMEOUT", runtime: "124.5000", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(new InstanceSeedFile("foo", 42), new CancellationToken());
            echoRun.Wait();

            var result = echoRun.Result;
            Assert.True(result.IsCancelled, "Result with status TIMEOUT should be marked as cancelled.");
            Assert.Equal(300, result.Runtime.TotalSeconds);
        }

        /// <summary>
        /// Checks that <see cref="RuntimeRunner"/> sets results with status CRASHED to cancelled and assigns them
        /// the maximum runtime.
        /// </summary>
        [Fact]
        public void CrashedResultsHaveMaximumRuntime()
        {
            var scenario = this.BuildResultEchoScenario("CRASHED", runtime: "4.5000", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(new InstanceSeedFile("foo", 42), new CancellationToken());
            echoRun.Wait();

            var result = echoRun.Result;
            Assert.True(result.IsCancelled, "Result with status CRASHED should be marked as cancelled.");
            Assert.Equal(300, result.Runtime.TotalSeconds);
        }

        /// <summary>
        /// Checks that <see cref="RuntimeRunner"/> sets results with (at least) maximum runtime to cancelled.
        /// </summary>
        [Fact]
        public void MaximumRuntimeResultsAreCancelled()
        {
            var scenario = this.BuildResultEchoScenario("SAT", runtime: "300", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(new InstanceSeedFile("foo", 42), new CancellationToken());
            echoRun.Wait();

            var result = echoRun.Result;
            Assert.True(result.IsCancelled, "Result with maximum runtime should be marked as cancelled.");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="RunnerBase{TResult}"/> to use in tests.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameters">The target algorithm parameters.</param>
        /// <returns>The created <see cref="RunnerBase{TResult}"/>.</returns>
        protected override RunnerBase<RuntimeResult> CreateRunner(Scenario scenario, Dictionary<string, IAllele> parameters)
        {
            return new RuntimeRunner(scenario, parameters);
        }

        /// <summary>
        /// Validates the exception thrown when a target algorithm run was cancelled.
        /// </summary>
        /// <param name="exception">The thrown exception.</param>
        protected override void InspectCancellationException(Exception exception)
        {
            Assert.Equal(
                typeof(TaskCanceledException),
                exception?.InnerException!.GetType());
        }

        #endregion
    }
}