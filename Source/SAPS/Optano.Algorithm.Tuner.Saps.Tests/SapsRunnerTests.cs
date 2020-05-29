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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="SapsRunner"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class SapsRunnerTests : IDisposable
    {
        #region Constants

        /// <summary>
        /// Path to ubcsat excutable.
        /// </summary>
        private const string PathToExecutable = @"Tools/ubcsat.exe";

        /// <summary>
        /// The path to the instance we run ubcsat on for testing.
        /// </summary>
        private const string PathToTestInstance = @"Tools/testInstance.cnf";

        /// <summary>
        /// The seed to the instance we run ubcsat on for testing.
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
        /// Initializes a new instance of the <see cref="SapsRunnerTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public SapsRunnerTests()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Checks that <see cref="SapsRunner.Run"/> can be cancelled.
        /// </summary>
        [Fact]
        public void CancellationWorks()
        {
            var timer = new Stopwatch();
            timer.Start();

            // Run SAPS.
            var sapsRunner = new SapsRunner(new Dictionary<string, IAllele>(), SapsRunnerTests.PathToExecutable, TimeSpan.FromMilliseconds(30000));
            var runner = sapsRunner.Run(
                new InstanceSeedFile(SapsRunnerTests.PathToTestInstance, SapsRunnerTests.TestInstanceSeed),
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
        /// Checks that <see cref="SapsRunner.CreateRuntimeResult"/> returns the correct result.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="timeoutLimit">The timeout limit.</param>
        [Theory]
        [InlineData(5, 10)]
        [InlineData(9.01, 10)]
        public void CreateRuntimeResultCreatesCorrectResult(double runtime, double timeoutLimit)
        {
            var output = $"CPUTime_Median = {runtime.ToString(CultureInfo.InvariantCulture)}";
            var timeout = TimeSpan.FromSeconds(timeoutLimit);
            var runtimeResult = SapsRunner.CreateRuntimeResult(output, timeout);
            runtimeResult.IsCancelled.ShouldBeFalse("Expected not cancelled result.");
            runtimeResult.Runtime.TotalSeconds.ShouldBe(runtime, 1e-6, "Expected different runtime in result.");
        }

        /// <summary>
        /// Checks that <see cref="SapsRunner.CreateRuntimeResult"/> returns a cancelled result, if a) the output does not fit to the regex or b) the runtime exceeds the timeout limit.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="timeoutLimit">The timeout limit.</param>
        [Theory]
        [InlineData("CPUTime_Median = 15", 10)]
        [InlineData("CPUTime_Mean = 15", 10)]
        public void CreateRuntimeResultCreatesCancelledResult(string output, double timeoutLimit)
        {
            var timeout = TimeSpan.FromSeconds(timeoutLimit);
            var runtimeResult = SapsRunner.CreateRuntimeResult(output, timeout);
            runtimeResult.IsCancelled.ShouldBeTrue("Expected cancelled result.");
            runtimeResult.Runtime.TotalSeconds.ShouldBe(timeoutLimit, "Expected different runtime in result.");
        }

        #endregion
    }
}