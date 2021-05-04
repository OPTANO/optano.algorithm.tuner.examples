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
    using System;
    using System.Linq;

    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Result of a (possibly cancelled) Gurobi run.
    /// </summary>
    public class GurobiResult : ResultBase<GurobiResult>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiResult" /> class.
        /// </summary>
        /// <param name="gap">The MIP gap at the end of the run.</param>
        /// <param name="runtime">The runtime in milliseconds.</param>
        /// <param name="targetAlgorithmStatus">The target algorithm status.</param>
        /// <param name="hasValidSolution">Whether a valid solution was found.</param>
        public GurobiResult(double gap, TimeSpan runtime, TargetAlgorithmStatus targetAlgorithmStatus, bool hasValidSolution)
            : base(runtime, targetAlgorithmStatus)
        {
            // Check some parameters.
            if (gap < 0)
            {
                throw new ArgumentOutOfRangeException($"Gap has to be nonnegative, but is {gap}.");
            }

            // Set them.
            this.Gap = gap;
            this.Runtime = runtime;
            this.HasValidSolution = hasValidSolution;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiResult" /> class.
        /// Empty ctor required for <see cref="ResultBase{TResultType}.CreateCancelledResult"/>.
        /// This ctor is never used, since this adapter handles its cancelled results on its own.
        /// </summary>
        public GurobiResult()
            : this(double.NaN, TimeSpan.MaxValue, TargetAlgorithmStatus.CancelledByTimeout, false)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the MIP gap at the end of the run.
        /// </summary>
        public double Gap { get; }

        /// <summary>
        /// Gets a value indicating whether the run completed with a feasible solution.
        /// </summary>
        public bool HasValidSolution { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <returns>String representation of the object.</returns>
        public override string ToString()
        {
            return
                $"Cancelled: {this.IsCancelled}; Runtime: {this.Runtime} ms; Found feasible solution: {this.HasValidSolution}; MIP gap: {this.Gap}";
        }

        /// <inheritdoc />
        public override string[] GetHeader()
        {
            return base.GetHeader().Concat(new[] { "MipGap", "HasValidSolution" }).ToArray();
        }

        /// <inheritdoc />
        public override string[] ToStringArray()
        {
            return base.ToStringArray().Concat(new[] { $"{this.Gap:0.######}", $"{this.HasValidSolution}" }).ToArray();
        }

        #endregion
    }
}