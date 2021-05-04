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
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="GurobiRunner"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiRunnerTests : IDisposable
    {
        #region Constants

        /// <summary>
        /// The path to the instance we run Gurobi on for testing.
        /// </summary>
        private const string PathToTestInstance = @"Tools/glass4.mps";

        /// <summary>
        /// The seed to the instance we run Gurobi on for testing.
        /// </summary>
        private const int TestInstanceSeed = 42;

        /// <summary>
        /// The path to the post tuning file we run Gurobi on for testing.
        /// </summary>
        private const string PathToPostTuningFile = @"Tools/gurobiPostTuningRuns.csv";

        #endregion

        #region Fields

        /// <summary>
        /// The data record folder used in tests.
        /// </summary>
        private readonly string _dataRecordDirectory = PathUtils.GetAbsolutePathFromCurrentDirectory("TestDirectory");

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunnerTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiRunnerTests()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Directory.Exists(this._dataRecordDirectory))
            {
                Directory.Delete(this._dataRecordDirectory, recursive: true);
            }
        }

        /// <summary>
        /// Smoke test for the Gurobi adapter.
        /// </summary>
        [Fact]
        public void SmokeTest()
        {
            var timer = Stopwatch.StartNew();
            var args = new[]
                           {
                               "--master", "--maxParallelEvaluations=1", "--trainingInstanceFolder=Tools", "--numGens=2", "--goalGen=0",
                               "--popSize=8", "--miniTournamentSize=4", "--cpuTimeout=1", "--instanceNumbers=1:1", "--enableRacing=True",
                           };
            Program.Main(args);
            timer.Stop();
            timer.Elapsed.TotalMilliseconds.ShouldBeGreaterThan(2000);
        }

        /// <summary>
        /// Smoke test for the post tuning runner of the Gurobi adapter.
        /// </summary>
        [Fact]
        public void PostTuningSmokeTest()
        {
            Directory.Exists(this._dataRecordDirectory).ShouldBeFalse();

            var timer = Stopwatch.StartNew();
            var args = new[]
                           {
                               "--postTuning", "--maxParallelEvaluations=1", "--cpuTimeout=1", "--enableDataRecording=True",
                               $"--dataRecordDirectory={this._dataRecordDirectory}",
                               $"--pathToPostTuningFile={GurobiRunnerTests.PathToPostTuningFile}",
                               "--indexOfFirstPostTuningRun=0", "--numberOfPostTuningRuns=2",
                           };
            Program.Main(args);
            timer.Stop();
            timer.Elapsed.ShouldBeGreaterThan(TimeSpan.FromMilliseconds(2000));

            Directory.Exists(this._dataRecordDirectory).ShouldBeTrue();
            File.Exists(
                Path.Combine(
                    this._dataRecordDirectory,
                    $"dataLog_generation_-1_process_{(object)ProcessUtils.GetCurrentProcessId()}_id_0_CancelledByTimeout.csv")).ShouldBeTrue();
            File.Exists(
                Path.Combine(
                    this._dataRecordDirectory,
                    $"dataLog_generation_-1_process_{(object)ProcessUtils.GetCurrentProcessId()}_id_1_CancelledByTimeout.csv")).ShouldBeTrue();
        }

        /// <summary>
        /// Checks that <see cref="GurobiRunner.Run"/> can be cancelled.
        /// </summary>
        [Fact]
        public void CancellationWorks()
        {
            var gurobiEnvironment = new GRBEnv();
            var runnerConfiguration =
                new GurobiRunnerConfiguration.GurobiRunnerConfigBuilder().Build(TimeSpan.FromSeconds(1));
            var tunerConfiguration = new AlgorithmTunerConfiguration();

            // Note, that this cancellation token source is never used in GurobiRunner.Run().
            var cancellationTokenSource = new CancellationTokenSource(500);

            var timer = new Stopwatch();
            timer.Start();

            var gurobiRunner = new GurobiRunner(gurobiEnvironment, runnerConfiguration, tunerConfiguration);
            var runner = gurobiRunner.Run(
                new InstanceSeedFile(GurobiRunnerTests.PathToTestInstance, GurobiRunnerTests.TestInstanceSeed),
                cancellationTokenSource.Token);

            runner.Wait();
            timer.Stop();
            var result = runner.Result;
            result.IsCancelled.ShouldBeTrue();
            timer.Elapsed.ShouldBeGreaterThan(TimeSpan.FromMilliseconds(1000));
            timer.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(1900));
            gurobiEnvironment.Dispose();
        }

        #endregion
    }
}