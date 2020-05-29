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

namespace Optano.Algorithm.Tuner.Lingeling.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="LingelingRunner"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class LingelingRunnerTests : IDisposable
    {
        #region Constants

        /// <summary>
        /// Path to lingeling executable.
        /// </summary>
        private const string PathToExecutable = @"Tools/lingeling";

        /// <summary>
        /// The path to the instance we run lingeling on for testing.
        /// </summary>
        private const string PathToTestInstance = @"Tools/testInstance.cnf";

        /// <summary>
        /// The seed to the instance we run lingeling on for testing.
        /// </summary>
        private const int TestInstanceSeed = 42;

        #endregion

        #region Fields

        /// <summary>
        /// A <see cref="CancellationTokenSource"/>.
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LingelingRunnerTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public LingelingRunnerTests()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Checks that <see cref="LingelingRunner.Run"/> can be cancelled.
        /// This test only works on a Linux machine.
        /// </summary>
        [SkippableFact]
        public void CancellationWorks()
        {
            // Check, if current OS is Linux.
            Skip.IfNot(Environment.OSVersion.Platform == PlatformID.Unix);

            var timeout = TimeSpan.FromSeconds(30);
            var memoryLimitMegabyte = 4000;
            var tunerConfig = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().SetCpuTimeout(timeout).Build(1);

            var timer = new Stopwatch();
            timer.Start();

            // Run Lingeling.
            var lingelingRunner = new LingelingRunner(
                new Dictionary<string, IAllele>(),
                LingelingRunnerTests.PathToExecutable,
                tunerConfig,
                memoryLimitMegabyte);
            var runner = lingelingRunner.Run(
                new InstanceSeedFile(LingelingRunnerTests.PathToTestInstance, LingelingRunnerTests.TestInstanceSeed),
                this._cancellationTokenSource.Token);

            // Cancel task and expect it to be cancelled.
            try
            {
                Thread.Sleep(100);
                this._cancellationTokenSource.Cancel();
                runner.Wait();
                Assert.True(false, "Expected a task cancelled exception.");
            }
            catch (AggregateException aggregateException)
            {
                timer.Stop();

                aggregateException.InnerExceptions.Count.ShouldBe(1);
                var innerException = aggregateException.InnerExceptions.Single();
                innerException.ShouldBeOfType<TaskCanceledException>();

                runner.IsCanceled.ShouldBeTrue();
                timer.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {
                Assert.True(false, "Expected a task cancelled exception.");
            }
        }

        /// <summary>
        /// Checks that <see cref="LingelingRunner.Run"/> is cancelled, if the memory limit is exceeded.
        /// This test only works on a Linux machine.
        /// </summary>
        [SkippableFact]
        public void MemoryLimitWorks()
        {
            // Check, if current OS is Linux.
            Skip.IfNot(Environment.OSVersion.Platform == PlatformID.Unix);

            var timeout = TimeSpan.FromSeconds(10);
            var memoryLimitMegabyte = 1;
            var tunerConfig = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().SetCpuTimeout(timeout).Build(1);

            var timer = new Stopwatch();
            timer.Start();

            var lingelingRunner = new LingelingRunner(
                new Dictionary<string, IAllele>(),
                LingelingRunnerTests.PathToExecutable,
                tunerConfig,
                memoryLimitMegabyte);
            var runner = lingelingRunner.Run(
                new InstanceSeedFile(LingelingRunnerTests.PathToTestInstance, LingelingRunnerTests.TestInstanceSeed),
                this._cancellationTokenSource.Token);

            runner.Wait();
            timer.Stop();
            runner.Result.ShouldNotBeNull();
            runner.Result.IsCancelled.ShouldBeTrue();
            runner.Result.Runtime.ShouldBe(timeout);
            timer.Elapsed.ShouldBeLessThan(timeout);
        }

        /// <summary>
        /// Checks that <see cref="LingelingRunner.ExtractRunStatistics" /> returns the correct result.
        /// </summary>
        /// <param name="consoleOutput">The console output.</param>
        [Theory]
        [InlineData(
            @"s SATISFIABLE
c 0.2 seconds, 1.5 MB")]
        public void ExtractRunStatisticsCreatesCorrectResult(string consoleOutput)
        {
            var timeout = TimeSpan.FromSeconds(5);
            var tunerConfig = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().SetCpuTimeout(timeout).Build(1);
            var lingelingRunner = new LingelingRunner(new Dictionary<string, IAllele>(), "ling", tunerConfig, 4000);

            var runResult = lingelingRunner.ExtractRunStatistics(consoleOutput);

            runResult.IsCancelled.ShouldBeFalse();
            runResult.Runtime.ShouldBe(TimeSpan.FromSeconds(0.2));
        }

        /// <summary>
        /// Checks that <see cref="LingelingRunner.ExtractRunStatistics" /> returns a cancelled result, if a) the console output does not fit to the regex or b) the runtime exceeds the timeout.
        /// </summary>
        /// <param name="consoleOutput">The console output.</param>
        [Theory]
        [InlineData(
            @"s SATIS
c 0.2 seconds, 1.5 MB")]
        [InlineData(
            @"s SATISFIABLE
c 0.2 sec, 1.5 MB")]
        [InlineData(
            @"s SATISFIABLE
c 5.2 seconds, 1.5 MB")]
        public void ExtractRunStatisticsCreatesCancelledResult(string consoleOutput)
        {
            var timeout = TimeSpan.FromSeconds(5);
            var tunerConfig = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().SetCpuTimeout(timeout).Build(1);
            var lingelingRunner = new LingelingRunner(new Dictionary<string, IAllele>(), "ling", tunerConfig, 4000);

            var runResult = lingelingRunner.ExtractRunStatistics(consoleOutput);

            runResult.IsCancelled.ShouldBeTrue();
            runResult.Runtime.ShouldBe(timeout);
        }

        #endregion
    }
}