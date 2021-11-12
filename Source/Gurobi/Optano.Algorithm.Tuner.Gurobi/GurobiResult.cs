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

    using global::Gurobi;

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
        /// <param name="bestObjective">The best objective at the end of the run.</param>
        /// <param name="bestObjectiveBound">The best objective bound at the end of the run.</param>
        /// <param name="runtime">The runtime in milliseconds.</param>
        /// <param name="targetAlgorithmStatus">The target algorithm status.</param>
        /// <param name="hasValidSolution">Whether a valid solution was found.</param>
        /// <param name="optimizationSenseIsMinimize">A value indicating whether the optimization sense is minimize.</param>
        public GurobiResult(
            double bestObjective,
            double bestObjectiveBound,
            TimeSpan runtime,
            TargetAlgorithmStatus targetAlgorithmStatus,
            bool hasValidSolution,
            bool optimizationSenseIsMinimize)
            : base(runtime, targetAlgorithmStatus)
        {
            this.BestObjective = bestObjective;
            this.BestObjectiveBound = bestObjectiveBound;
            this.Runtime = runtime;
            this.HasValidSolution = hasValidSolution;
            this.OptimizationSenseIsMinimize = optimizationSenseIsMinimize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiResult" /> class.
        /// Empty ctor required for <see cref="ResultBase{TResultType}.CreateCancelledResult"/>.
        /// This ctor is never used, since this adapter handles its cancelled results on its own.
        /// </summary>
        public GurobiResult()
            : this(GRB.INFINITY, -GRB.INFINITY, TimeSpan.MaxValue, TargetAlgorithmStatus.CancelledByTimeout, false, true)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the MIP gap at the end of the run.
        /// </summary>
        public double Gap => GurobiUtils.GetMipGap(this.BestObjective, this.BestObjectiveBound);

        /// <summary>
        /// Gets the best objective at the end of the run.
        /// </summary>
        public double BestObjective { get; }

        /// <summary>
        /// Gets the best objective bound at the end of the run.
        /// </summary>
        public double BestObjectiveBound { get; }

        /// <summary>
        /// Gets a value indicating whether the run completed with a feasible solution.
        /// </summary>
        public bool HasValidSolution { get; }

        /// <summary>
        /// Gets a value indicating whether the optimization sense is minimize.
        /// </summary>
        public bool OptimizationSenseIsMinimize { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        /// <returns>String representation of the object.</returns>
        public override string ToString()
        {
            return
                $"Cancelled: {this.IsCancelled}; Runtime: {this.Runtime} ms; Found feasible solution: {this.HasValidSolution}; MIP gap: {this.Gap}; Best objective: {this.BestObjective}; Best objective bound: {this.BestObjectiveBound}; Optimization sense: {(this.OptimizationSenseIsMinimize ? "Min" : "Max")};";
        }

        /// <inheritdoc />
        public override string[] GetHeader()
        {
            return base.GetHeader()
                .Concat(new[] { "MipGap", "BestObjective", "BestObjectiveBound", "HasValidSolution", "OptimizationSenseIsMinimize" }).ToArray();
        }

        /// <inheritdoc />
        public override string[] ToStringArray()
        {
            return base.ToStringArray().Concat(
                    new[]
                        {
                            $"{this.Gap:0.######}", $"{this.BestObjective:0.######}", $"{this.BestObjectiveBound:0.######}",
                            $"{this.HasValidSolution}", $"{this.OptimizationSenseIsMinimize}",
                        })
                .ToArray();
        }

        #endregion
    }
}