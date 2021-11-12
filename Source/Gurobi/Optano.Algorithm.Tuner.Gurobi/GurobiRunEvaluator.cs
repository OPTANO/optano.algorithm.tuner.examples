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
    using System.Collections.Generic;
    using System.Linq;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.GenomeEvaluation.Evaluation;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.RunEvaluators;

    /// <summary>
    /// An implementation of <see cref="IRunEvaluator{TInstance,TResult}" /> that sorts genomes according to their <see cref="GurobiResult"/>s.
    /// </summary>
    public class GurobiRunEvaluator : RacingRunEvaluatorBase<InstanceSeedFile, GurobiResult>
    {
        #region Fields

        /// <summary>
        /// The tertiary tune criterion.
        /// </summary>
        private readonly GurobiTertiaryTuneCriterion _tertiaryTuneCriterion;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunEvaluator"/> class.
        /// </summary>
        /// <param name="cpuTimeout">The cpu timeout.</param>
        /// <param name="tertiaryTuneCriterion">The tertiary tune criterion.</param>
        public GurobiRunEvaluator(TimeSpan cpuTimeout, GurobiTertiaryTuneCriterion tertiaryTuneCriterion)
            : base(cpuTimeout, null)
        {
            this._tertiaryTuneCriterion = tertiaryTuneCriterion;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        /// <remarks>
        /// Since the best gurobi result is a finished result, its best objective and best objective bound are not relevant.
        /// </remarks>
        protected override GurobiResult BestPossibleResult => new GurobiResult(
            double.NaN,
            double.NaN,
            TimeSpan.Zero,
            TargetAlgorithmStatus.Finished,
            true,
            true);

        /// <inheritdoc />
        protected override GurobiResult WorstPossibleResult => new GurobiResult(
            GRB.INFINITY,
            -GRB.INFINITY,
            this.CpuTimeout,
            TargetAlgorithmStatus.CancelledByTimeout,
            false,
            true);

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public override IEnumerable<ImmutableGenomeStats<InstanceSeedFile, GurobiResult>> Sort(
            IEnumerable<ImmutableGenomeStats<InstanceSeedFile, GurobiResult>> allGenomeStatsOfMiniTournament)
        {
            /* This implementation uses the following sorting criteria:

            1.) The higher the number of results with valid solution, the better.
            2.) The lower the number of cancelled results, the better.
            3.) The lower the tertiary tune criterion, the better.
            4.) The lower the averaged runtime, the better.

            NOTE: No need to penalize the average runtime, since the number of cancelled results is a superior sorting criterion.*/

            return allGenomeStatsOfMiniTournament
                .OrderByDescending(gs => gs.FinishedInstances.Values.Count(result => result.HasValidSolution))
                .ThenBy(gs => gs.FinishedInstances.Values.Count(result => result.IsCancelled))
                .ThenBy(
                    gs => gs.FinishedInstances.Values
                        .Where(result => result.IsCancelled)
                        .Select(this.GetTertiaryTuneCriterionValueToMinimize)
                        .DefaultIfEmpty(GRB.INFINITY)
                        .Average())
                .ThenBy(
                    gs => gs.FinishedInstances.Values
                        .Select(result => result.Runtime.TotalSeconds)
                        .DefaultIfEmpty(TimeSpan.MaxValue.TotalSeconds)
                        .Average());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the tertiary tune criterion value to minimize.
        /// </summary>
        /// <param name="gurobiResult">The gurobi result.</param>
        /// <returns>The tertiary tune criterion.</returns>
        private double GetTertiaryTuneCriterionValueToMinimize(GurobiResult gurobiResult)
        {
            return this._tertiaryTuneCriterion switch
                {
                    GurobiTertiaryTuneCriterion.MipGap => gurobiResult.Gap,
                    GurobiTertiaryTuneCriterion.BestObjective => gurobiResult.OptimizationSenseIsMinimize
                                                                     ? gurobiResult.BestObjective
                                                                     : -gurobiResult.BestObjective,
                    GurobiTertiaryTuneCriterion.BestObjectiveBound => gurobiResult.OptimizationSenseIsMinimize
                                                                          ? -gurobiResult.BestObjectiveBound
                                                                          : gurobiResult.BestObjectiveBound,
                    GurobiTertiaryTuneCriterion.None => 42,
                    _ => throw new NotImplementedException(
                             $"The tertiary tune criterion value to minimize is not implemented for {this._tertiaryTuneCriterion}!"),
                };
        }

        #endregion
    }
}