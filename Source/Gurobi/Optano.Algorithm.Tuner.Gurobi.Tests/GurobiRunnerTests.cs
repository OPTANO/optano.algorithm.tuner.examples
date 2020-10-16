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

namespace Optano.Algorithm.Tuner.Gurobi.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using global::Gurobi;

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

            // Note, that this cancellation token source is never used in GurobiRunner.Run().
            var cancellationTokenSource = new CancellationTokenSource(500);

            var timer = new Stopwatch();
            timer.Start();

            var gurobiRunner = new GurobiRunner(gurobiEnvironment, runnerConfiguration);
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