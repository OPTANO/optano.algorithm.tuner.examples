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
    using System.Linq;

    using Optano.Algorithm.Tuner.GrayBox.DataRecordTypes;
    using Optano.Algorithm.Tuner.Gurobi.GurobiAdapterFeatures;
    using Optano.Algorithm.Tuner.MachineLearning.GenomeRepresentation;
    using Optano.Algorithm.Tuner.TargetAlgorithm;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="GurobiGrayBoxMethods"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiGrayBoxMethodsTests : IDisposable

    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiGrayBoxMethodsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiGrayBoxMethodsTests()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Checks that the correct gray box features are selected.
        /// </summary>
        [Fact]
        public void CorrectGrayBoxFeaturesAreSelected()
        {
            var gurobiGrayBoxMethods = new GurobiGrayBoxMethods();

            var runtimeFeatures = new GurobiRuntimeFeatures(DateTime.Now);
            var instanceFeatures = new GurobiInstanceFeatures();
            var adapterFeatures = GurobiUtils.ComposeAdapterFeatures(runtimeFeatures, runtimeFeatures, instanceFeatures);
            var adapterFeaturesHeader = GurobiUtils.ComposeAdapterFeaturesHeader(runtimeFeatures, runtimeFeatures, instanceFeatures);
            var result = new GurobiResult(double.NaN, TimeSpan.MaxValue, TargetAlgorithmStatus.CancelledByTimeout, false);

            var adapterDataRecord = new AdapterDataRecord<GurobiResult>(
                "Gurobi901",
                TargetAlgorithmStatus.Running,
                TimeSpan.Zero,
                TimeSpan.Zero,
                DateTime.Now,
                adapterFeaturesHeader,
                adapterFeatures,
                result);

            var tunerDataRecord = new TunerDataRecord<GurobiResult>(
                "Node",
                0,
                0,
                "Instance",
                0.5,
                new[] { "Genome" },
                (GenomeDoubleRepresentation)new[] { 0D },
                result);

            var dataRecord = new DataRecord<GurobiResult>(tunerDataRecord, adapterDataRecord);

            var featureNames = gurobiGrayBoxMethods.GetGrayBoxFeatureNamesFromDataRecord(dataRecord);

            var correctFeatureNames = new[]
                                          {
                                              "ExpendedWallClockTime",
                                              "RuntimeFeature_CuttingPlanesCount_Current",
                                              "RuntimeFeature_ExploredNodeCount_Current",
                                              "RuntimeFeature_FeasibleSolutionsCount_Current",
                                              "RuntimeFeature_MipGap_Current",
                                              "RuntimeFeature_SimplexIterationsCount_Current",
                                              "RuntimeFeature_UnexploredNodeCount_Current",
                                              "RuntimeFeature_CuttingPlanesCount_Last",
                                              "RuntimeFeature_ExploredNodeCount_Last",
                                              "RuntimeFeature_FeasibleSolutionsCount_Last",
                                              "RuntimeFeature_MipGap_Last",
                                              "RuntimeFeature_SimplexIterationsCount_Last",
                                              "RuntimeFeature_UnexploredNodeCount_Last",
                                              "InstanceFeature_NumberOfIntegerVariables",
                                              "InstanceFeature_NumberOfLinearConstraints",
                                              "InstanceFeature_NumberOfNonZeroCoefficients",
                                              "InstanceFeature_NumberOfVariables",
                                          };

            featureNames.SequenceEqual(correctFeatureNames).ShouldBeTrue();
        }

        #endregion
    }
}