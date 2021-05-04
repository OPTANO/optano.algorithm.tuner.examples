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

namespace Optano.Algorithm.Tuner.AcLib.Tests.TargetAlgorithm.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Quality;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="QualityRunner"/> class.
    /// </summary>
    public class QualityRunnerTest : RunnerBaseTest<ContinuousResult>
    {
        #region Public Methods and Operators

        /// <summary>
        /// Checks that crashed target algorithm runs throw an exception.
        /// </summary>
        [Fact]
        public void CrashedRunThrowsException()
        {
            var scenario = this.BuildResultEchoScenario("CRASHED", runtime: "124.5000", quality: "5E-2");
            var runner = this.CreateRunner(scenario, new Dictionary<string, IAllele>());

            var echoRun = runner.Run(new InstanceSeedFile("foo", 42), new CancellationToken());
            Assert.Throws<AggregateException>(() => echoRun.Wait());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="QualityRunner" /> to use in tests.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameters">The target algorithm parameters.</param>
        /// <returns>
        /// The created <see cref="QualityRunner" />.
        /// </returns>
        protected override RunnerBase<ContinuousResult> CreateRunner(Scenario scenario, Dictionary<string, IAllele> parameters)
        {
            return new QualityRunner(scenario, parameters);
        }

        /// <summary>
        /// Validates the exception thrown when a target algorithm run was cancelled.
        /// </summary>
        /// <param name="exception">The thrown exception.</param>
        protected override void InspectCancellationException(Exception exception)
        {
            Assert.Equal(
                typeof(InvalidOperationException),
                exception.InnerException?.GetType());
        }

        #endregion
    }
}