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

namespace Optano.Algorithm.Tuner.Gurobi.GurobiAdapterFeatures
{
    using Optano.Algorithm.Tuner.GrayBox.DataRecordTypes;

    /// <summary>
    /// Summarizes all interesting Gurobi instance features.
    /// </summary>
    public class GurobiInstanceFeatures : AdapterFeaturesBase
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the number of variables.
        /// </summary>
        public double NumberOfVariables { get; set; }

        /// <summary>
        /// Gets or sets the number of linear constraints.
        /// </summary>
        public double NumberOfLinearConstraints { get; set; }

        /// <summary>
        /// Gets or sets the number of SOS constraints.
        /// </summary>
        public double NumberOfSosConstraints { get; set; }

        /// <summary>
        /// Gets or sets the number of quadratic constraints.
        /// </summary>
        public double NumberOfQuadraticConstraints { get; set; }

        /// <summary>
        /// Gets or sets the number of general constraints.
        /// </summary>
        public double NumberOfGeneralConstraints { get; set; }

        /// <summary>
        /// Gets or sets the number of non zero coefficients.
        /// </summary>
        public double NumberOfNonZeroCoefficients { get; set; }

        /// <summary>
        /// Gets or sets the number of non zero quadratic objective terms.
        /// </summary>
        public double NumberOfNonZeroQuadraticObjectiveTerms { get; set; }

        /// <summary>
        /// Gets or sets the number of non zero terms in quadratic constraints.
        /// </summary>
        public double NumberOfNonZeroTermsInQuadraticConstraints { get; set; }

        /// <summary>
        /// Gets or sets the number of integer variables.
        /// </summary>
        public double NumberOfIntegerVariables { get; set; }

        /// <summary>
        /// Gets or sets the number of binary variables.
        /// </summary>
        public double NumberOfBinaryVariables { get; set; }

        /// <summary>
        /// Gets or sets the number of variables with piecewise linear objective functions.
        /// </summary>
        public double NumberOfVariablesWithPiecewiseLinearObjectiveFunctions { get; set; }

        #endregion
    }
}