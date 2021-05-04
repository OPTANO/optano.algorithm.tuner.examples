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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System.Linq;

    using Optano.Algorithm.Tuner.GrayBox;
    using Optano.Algorithm.Tuner.GrayBox.DataRecordTypes;

    /// <summary>
    /// Custom implementation of the <see cref="ICustomGrayBoxMethods{TResult}"/>.
    /// </summary>
    public class GurobiGrayBoxMethods : ICustomGrayBoxMethods<GurobiResult>
    {
        #region Fields

        /// <summary>
        /// The indices of the <see cref="AdapterDataRecord{TResult}.AdapterFeatures"/>, used in gray box tuning.
        /// </summary>
        private readonly int[] _grayBoxAdapterFeatureIndices =
            {
                // Select interesting current runtime features.
                3, // CuttingPlanesCount
                4, // ExploredNodeCount
                5, // FeasibleSolutionsCount
                6, // MipGap
                12, // SimplexIterationsCount
                14, // UnexploredNodeCount

                // Select interesting last runtime features.
                18, // CuttingPlanesCount
                19, // ExploredNodeCount
                20, // FeasibleSolutionsCount
                21, // MipGap
                27, // SimplexIterationsCount
                29, // UnexploredNodeCount

                // Select interesting instance features.
                32, // NumberOfIntegerVariables
                33, // NumberOfLinearConstraints
                34, // NumberOfNonZeroCoefficients
                39, // NumberOfVariables
            };

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public double[] GetGrayBoxFeaturesFromDataRecord(DataRecord<GurobiResult> dataRecord)
        {
            var otherGrayBoxFeatures = new double[] { dataRecord.AdapterDataRecord.ExpendedWallClockTime.TotalMilliseconds };
            var grayBoxAdapterFeatures =
                this._grayBoxAdapterFeatureIndices.Select(index => dataRecord.AdapterDataRecord.AdapterFeatures[index]);
            return otherGrayBoxFeatures.Concat(grayBoxAdapterFeatures).ToArray();
        }

        /// <inheritdoc />
        public string[] GetGrayBoxFeatureNamesFromDataRecord(DataRecord<GurobiResult> dataRecord)
        {
            var otherGrayBoxNames = new string[] { "ExpendedWallClockTime" };
            var grayBoxAdapterFeatureNames =
                this._grayBoxAdapterFeatureIndices.Select(index => dataRecord.AdapterDataRecord.AdapterFeaturesHeader[index]);
            return otherGrayBoxNames.Concat(grayBoxAdapterFeatureNames).ToArray();
        }

        #endregion
    }
}