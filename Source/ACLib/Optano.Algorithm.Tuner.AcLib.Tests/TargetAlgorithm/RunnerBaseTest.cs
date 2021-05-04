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

namespace Optano.Algorithm.Tuner.AcLib.Tests.TargetAlgorithm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm;
    using Optano.Algorithm.Tuner.AcLib.Tests.Configuration;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    using Xunit;

    /// <summary>
    /// Base class for all test classes which test subclasses of <see cref="RunnerBase{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the target algorithm run result wrapper.</typeparam>
    [Collection("NonParallel")]
    [SuppressMessage(
        "NDepend",
        "ND2003:AbstractBaseClassShouldBeSuffixedWithBase",
        Justification = "This is a base test class.")]
    public abstract class RunnerBaseTest<TResult> : IDisposable
        where TResult : ResultBase<TResult>, new()
    {
        #region Constants

        /// <summary>
        /// Path for ACLib scenario file.
        /// </summary>
        private const string ScenarioSpecificationFile = "specification.txt";

        #endregion

        #region Fields

        /// <summary>
        /// An arbitrary <see cref="InstanceSeedFile"/>.
        /// </summary>
        private readonly InstanceSeedFile _instanceSeedFile = new InstanceSeedFile("foo", 42);

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
            if (File.Exists(RunnerBaseTest<TResult>.ScenarioSpecificationFile))
            {
                File.Delete(RunnerBaseTest<TResult>.ScenarioSpecificationFile);
            }

            if (Directory.Exists("output"))
            {
                Directory.Delete("output", recursive: true);
            }
        }

        /// <summary>
        /// Checks that cancelling a target algorithm run and handling its result works as expected.
        /// </summary>
        [Fact]
        public void CancelledRunIsHandledCorrectly()
        {
            // Create long-running scenario.
            var scenarioFileWriter = new ScenarioFileWriter
                                         {
                                             CutoffTime = "300",
                                             Command = "cmd.exe /c timeout 10",
                                             InstanceFile = "does not matter",
                                             ParameterFile = "does not matter",
                                             OverallObjective = "does not matter",
                                             RunObjective = "does not matter",
                                         };
            scenarioFileWriter.Write(RunnerBaseTest<TResult>.ScenarioSpecificationFile);
            var runner = this.CreateRunner(new Scenario(RunnerBaseTest<TResult>.ScenarioSpecificationFile), new Dictionary<string, IAllele>());

            // Add a cancellation token with short timeout.
            try
            {
                var echoRun = runner.Run(
                    this._instanceSeedFile,
                    new CancellationTokenSource(TimeSpan.FromMilliseconds(10)).Token);
                echoRun.Wait();
            }
            catch (Exception e)
            {
                this.InspectCancellationException(e);
            }
        }

        /// <summary>
        /// Checks that, in the case of a successful run, the correct result is created from the line that is printed
        /// out for ParamILS.
        /// </summary>
        [Fact]
        public virtual void RunResultIsCreatedFromParamIlsLine()
        {
            var scenario = this.BuildResultEchoScenario("SAT", runtime: "124.5000", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(this._instanceSeedFile, new CancellationToken());
            echoRun.Wait();

            var result = echoRun.Result;
            Assert.Equal(TimeSpan.FromSeconds(124.5), result.Runtime);
            Assert.False(result.IsCancelled, "Status was read correctly.");
        }

        /// <summary>
        /// Checks that the runner throws an exception if the target algorithm returns an invalid
        /// status value.
        /// </summary>
        [Fact]
        public void InvalidReturnStatusThrowsException()
        {
            var scenario = this.BuildResultEchoScenario("INVALID", runtime: "124.5000", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(this._instanceSeedFile, new CancellationToken());
            Assert.Throws<AggregateException>(() => echoRun.Wait());
        }

        /// <summary>
        /// Checks that the runner throws an exception if the target algorithm returns an invalid
        /// runtime value.
        /// </summary>
        [Fact]
        public void InvalidRuntimeThrowsException()
        {
            var scenario = this.BuildResultEchoScenario("SAT", runtime: "124.50f0", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(this._instanceSeedFile, new CancellationToken());
            Assert.Throws<AggregateException>(() => echoRun.Wait());
        }

        /// <summary>
        /// Checks that the runner throws an <see cref="ArgumentException"/> if the target algorithm returns an invalid
        /// quality value.
        /// </summary>
        [Fact]
        public void InvalidQualityThrowsException()
        {
            var scenario = this.BuildResultEchoScenario("SAT", runtime: "124.5000", quality: "5h");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(this._instanceSeedFile, new CancellationToken());
            Assert.Throws<AggregateException>(() => echoRun.Wait());
        }

        /// <summary>
        /// Checks that target algorithm runs with status ABORT provoke an exception.
        /// </summary>
        [Fact]
        public void AbortStatusThrowsException()
        {
            var scenario = this.BuildResultEchoScenario("ABORT", runtime: "124.5000", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(this._instanceSeedFile, new CancellationToken());
            Assert.Throws<AggregateException>(() => echoRun.Wait());
        }

        /// <summary>
        /// Checks that missing result output throws an exception.
        /// </summary>
        [Fact]
        public void MissingOutputThrowsException()
        {
            // Use a command which does not print the complete result line.
            var scenarioFileWriter = new ScenarioFileWriter
                                         {
                                             CutoffTime = "300",
                                             Command = "cmd.exe /c echo Result for ParamILS: ",
                                             InstanceFile = "does not matter",
                                             ParameterFile = "does not matter",
                                             OverallObjective = "does not matter",
                                             RunObjective = "does not matter",
                                         };
            scenarioFileWriter.Write(RunnerBaseTest<TResult>.ScenarioSpecificationFile);
            var runner = this.CreateRunner(new Scenario(RunnerBaseTest<TResult>.ScenarioSpecificationFile), new Dictionary<string, IAllele>());

            var echoRun = runner.Run(this._instanceSeedFile, new CancellationToken());
            Assert.Throws<AggregateException>(() => echoRun.Wait());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="RunnerBase{TResult}"/> to use in tests.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameters">The target algorithm parameters.</param>
        /// <returns>The created <see cref="RunnerBase{TResult}"/>.</returns>
        protected abstract RunnerBase<TResult> CreateRunner(Scenario scenario, Dictionary<string, IAllele> parameters);

        /// <summary>
        /// Validates the exception thrown when a target algorithm run was cancelled.
        /// </summary>
        /// <param name="exception">The thrown exception.</param>
        protected abstract void InspectCancellationException(Exception exception);

        /// <summary>
        /// Creates a <see cref="Scenario"/> which echoes a result.
        /// </summary>
        /// <param name="status">The status to echo.</param>
        /// <param name="runtime">The runtime to echo.</param>
        /// <param name="quality">The quality to echo.</param>
        /// <returns>The created <see cref="Scenario"/>.</returns>
        protected Scenario BuildResultEchoScenario(string status, string runtime, string quality)
        {
            var scenarioFileWriter = new ScenarioFileWriter
                                         {
                                             CutoffTime = "300",
                                             Command = $"cmd.exe /c echo Result for ParamILS: {status}, {runtime}, 0.1, {quality}, 24",
                                             InstanceFile = "does not matter",
                                             ParameterFile = "does not matter",
                                             OverallObjective = "does not matter",
                                             RunObjective = "does not matter",
                                         };
            scenarioFileWriter.Write(RunnerBaseTest<TResult>.ScenarioSpecificationFile);
            return new Scenario(RunnerBaseTest<TResult>.ScenarioSpecificationFile);
        }

        #endregion
    }
}