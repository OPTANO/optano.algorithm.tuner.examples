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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="GurobiRunnerFactory"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiRunnerFactoryTests : IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunnerFactoryTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiRunnerFactoryTests()
        {
            ProcessUtils.SetDefaultCultureInfo(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Checks, that <see cref="GurobiRunnerFactory.TryToGetResultFromStringArray"/> works for <see cref="GurobiResult"/>s.
        /// </summary>
        [SuppressMessage(
            "NDepend",
            "ND2016:MethodsPrefixedWithTryShouldReturnABoolean",
            Justification = "This is a test method.")]
        [Fact]
        public void TryToGetResultFromStringArrayWorksForGurobiResults()
        {
            var timeout = TimeSpan.FromSeconds(30);
            var gurobiResult = new GurobiResult(2.5, -5, timeout, TargetAlgorithmStatus.CancelledByTimeout, true, true);
            var gurobiConfiguration = new GurobiRunnerConfiguration.GurobiRunnerConfigBuilder().Build(timeout);
            var tunerConfiguration = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().SetCpuTimeout(timeout)
                .Build(1);
            var targetAlgorithmFactory =
                new GurobiRunnerFactory(gurobiConfiguration, tunerConfiguration) as
                    ITargetAlgorithmFactory<GurobiRunner, InstanceSeedFile, GurobiResult>;
            targetAlgorithmFactory.TryToGetResultFromStringArray(gurobiResult.ToStringArray(), out var result).ShouldBeTrue();
            result.TargetAlgorithmStatus.ShouldBe(gurobiResult.TargetAlgorithmStatus);
            result.IsCancelled.ShouldBe(gurobiResult.IsCancelled);
            result.Runtime.ShouldBe(gurobiResult.Runtime);
            result.Gap.ShouldBe(gurobiResult.Gap);
            result.BestObjective.ShouldBe(gurobiResult.BestObjective);
            result.BestObjectiveBound.ShouldBe(gurobiResult.BestObjectiveBound);
            result.HasValidSolution.ShouldBe(gurobiResult.HasValidSolution);
            result.OptimizationSenseIsMinimize.ShouldBe(gurobiResult.OptimizationSenseIsMinimize);
        }

        #endregion
    }
}