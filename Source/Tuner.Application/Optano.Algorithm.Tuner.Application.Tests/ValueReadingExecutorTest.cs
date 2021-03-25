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

namespace Optano.Algorithm.Tuner.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ValueReadingExecutor"/> class.
    /// Should not be executed in parallel s.t. <see cref="RuntimeMeasureIsApproximatelyCorrect"/> has a chance.
    /// </summary>
    [Collection("NonParallel")]
    public class ValueReadingExecutorTest
    {
        #region Fields

        /// <summary>
        /// A <see cref="CancellationTokenSource"/>.
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Checks that the <see cref="ContinuousResult"/> returned by
        /// <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> contains an approximately
        /// correct runtime.
        /// </summary>
        [Fact]
        public void RuntimeMeasureIsApproximatelyCorrect()
        {
            // Set a cutoff time.
            var seconds = 1;

            // Run the command.
            var commandExecutor = ValueReadingExecutorTest.CreateSapsRunner(timeout: seconds);
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);
            runner.Wait();

            // Check the result.
            TestUtils.Equals(runner.Result.Runtime.TotalSeconds, seconds, seconds * 0.2);
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> can be cancelled.
        /// </summary>
        [Fact]
        public void CancellationWorks()
        {
            // Create executor that runs a 3s program.
            int seconds = 3;
            var basicCommand = $"timeout -t {seconds}";
            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, TimeSpan.FromSeconds(seconds));

            // Start it.
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);

            // Add verification of cancellation after end of task.
            var verification = runner.ContinueWith(tr => Assert.True(tr.IsCanceled));

            // Cancel task.
            this._cancellationTokenSource.Cancel();

            // Make sure verification runs.
            verification.Wait();
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> reads the very last 
        /// number printed by the program.
        /// </summary>
        [Fact]
        public void LastValueIsRead()
        {
            var input = "1 foo 6.5 bar";
            var basicCommand = $"{TestUtils.ReturnInputApplicationCall} {input}";
            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, TimeSpan.FromSeconds(5));
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);
            runner.Wait();

            Assert.Equal(6.5, runner.Result.Value);
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> can read values in 
        /// integer format.
        /// </summary>
        [Fact]
        public void IntegerCanBeRead()
        {
            var input = "1";
            var basicCommand = $"{TestUtils.ReturnInputApplicationCall} {input}";
            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, TimeSpan.FromSeconds(5));
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);
            runner.Wait();

            Assert.Equal(1, runner.Result.Value);
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> can read values in 
        /// negative double format.
        /// </summary>
        [Fact]
        public void NegativeDoubleCanBeRead()
        {
            var input = "-1.0";
            var basicCommand = $"{TestUtils.ReturnInputApplicationCall} {input}";
            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, TimeSpan.FromSeconds(5));
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);
            runner.Wait();

            Assert.Equal(-1.0, runner.Result.Value);
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> can read values in 
        /// +5e-7 format.
        /// </summary>
        [Fact]
        public void FormatWithECanBeRead()
        {
            var input = "+1e-2";
            var basicCommand = $"{TestUtils.ReturnInputApplicationCall} {input}";
            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, TimeSpan.FromSeconds(5));
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);
            runner.Wait();

            Assert.Equal(0.01, runner.Result.Value);
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> uses the parameters
        /// provided on construction.
        /// </summary>
        [Fact]
        public void ParametersAreUsed()
        {
            // Create command that simply echos the parameters at the end.
            var basicCommand = $"{TestUtils.ReturnInputApplicationCall} {ValueReadingExecutor.ParameterReplacement}";

            // Construct executor with a parameter which reads as a number.
            var parameters = new Dictionary<string, IAllele> { { "parameter", new Allele<string>("42") } };
            var commandExecutor = new ValueReadingExecutor(parameters, basicCommand, TimeSpan.FromSeconds(5));

            // Run it.
            var runner = commandExecutor.Run(new InstanceFile(""), this._cancellationTokenSource.Token);
            runner.Wait();

            // Check the number was read.
            Assert.Equal(42, runner.Result.Value);
        }

        /// <summary>
        /// Checks that <see cref="ValueReadingExecutor.Run(InstanceFile, CancellationToken)"/> uses the provided
        /// instance.
        /// </summary>
        [Fact]
        public void InstanceIsUsed()
        {
            // Create command that simply echos the instance path at the end.
            var basicCommand = $"{TestUtils.ReturnInputApplicationCall} {ValueReadingExecutor.InstanceReplacement}";
            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, TimeSpan.FromSeconds(5));

            // Use instance path which is a number.
            var runner = commandExecutor.Run(new InstanceFile("42"), this._cancellationTokenSource.Token);
            runner.Wait();

            // Verify it was fetched.
            Assert.Equal(42, runner.Result.Value);
        }

        /// <summary>
        /// Checks, that the <see cref="ValueReadingExecutor"/> creates a cancelled result, if the process returns 1 as exit code.
        /// </summary>
        [Fact]
        public void ValueReadingExecutorCreatesCancelledResult()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var exitCode = 1;
            var basicCommand = $"{TestUtils.ReturnExitCodeApplicationCall} {exitCode}";
            var instance = new InstanceFile("");

            var timer = new Stopwatch();
            timer.Start();

            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, timeout);
            var runner = commandExecutor.Run(instance, this._cancellationTokenSource.Token);

            runner.Wait();
            timer.Stop();
            runner.Result.ShouldNotBeNull();
            runner.Result.IsCancelled.ShouldBeTrue();
            runner.Result.Runtime.ShouldBe(timeout);
            timer.Elapsed.ShouldBeLessThan(timeout);
        }

        /// <summary>
        /// Checks, that the <see cref="ValueReadingExecutor"/> creates not a cancelled result, if the process returns 0 as exit code.
        /// </summary>
        [Fact]
        public void ValueReadingExecutorCreatesNotCancelledResult()
        {
            var timeout = TimeSpan.FromSeconds(10);
            var exitCode = 0;
            var basicCommand = $"{TestUtils.ReturnExitCodeApplicationCall} {exitCode}";
            var instance = new InstanceFile("");

            var timer = new Stopwatch();
            timer.Start();

            var commandExecutor = new ValueReadingExecutor(new Dictionary<string, IAllele>(), basicCommand, timeout);
            var runner = commandExecutor.Run(instance, this._cancellationTokenSource.Token);

            runner.Wait();
            timer.Stop();
            runner.Result.ShouldNotBeNull();
            runner.Result.IsCancelled.ShouldBeFalse();
            runner.Result.Runtime.ShouldBeLessThan(timeout);
            timer.Elapsed.ShouldBeLessThan(timeout);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a <see cref="ValueReadingExecutor"/> executing SAPS on an instance that takes several seconds to
        /// solve.
        /// </summary>
        /// <param name="timeout">Timeout in seconds for the SAPS algorithm.</param>
        /// <returns>The created <see cref="ValueReadingExecutor"/>.</returns>
        private static ValueReadingExecutor CreateSapsRunner(int timeout)
        {
            var fixedParameters = @"-alg saps -seed 0 -cutoff max";
            var basicCommand =
                $@"{TestUtils.PathToTargetAlgorithm} {fixedParameters} -i {TestUtils.PathToTestInstance} {ValueReadingExecutor.ParameterReplacement}";
            var parameters = new Dictionary<string, IAllele>(capacity: 1) { { "timeout", new Allele<int>(timeout) } };
            return new ValueReadingExecutor(parameters, basicCommand, TimeSpan.FromSeconds(timeout));
        }

        #endregion
    }
}