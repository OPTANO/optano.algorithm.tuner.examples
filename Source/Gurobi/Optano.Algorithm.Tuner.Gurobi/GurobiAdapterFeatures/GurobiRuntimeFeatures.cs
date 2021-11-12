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
    using System;

    using Optano.Algorithm.Tuner.GrayBox.DataRecordTypes;

    /// <summary>
    /// Summarizes all interesting Gurobi runtime features.
    /// </summary>
    public class GurobiRuntimeFeatures : AdapterFeaturesBase
    {
        #region Fields

        /// <summary>
        /// The immutable version of <see cref="TimeSinceLastCallback"/>.
        /// </summary>
        private double? _immutableTimeSinceLastCallback;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRuntimeFeatures" /> class.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="optimizationSenseIsMinimize">A value indicating whether the optimization sense is minimize.</param>
        public GurobiRuntimeFeatures(DateTime timeStamp, bool optimizationSenseIsMinimize)
        {
            this._immutableTimeSinceLastCallback = null;
            this.OptimizationSenseIsMinimize = optimizationSenseIsMinimize;
            this.TimeStampOfLastEditing = timeStamp;
            this.BestObjective = GurobiUtils.GetBestObjectiveFallback(optimizationSenseIsMinimize);
            this.BestObjectiveBound = GurobiUtils.GetBestObjectiveBoundFallback(optimizationSenseIsMinimize);
            this.FeasibleSolutionsCount = 0;
            this.ExploredNodeCount = 0;
            this.UnexploredNodeCount = 0;
            this.BarrierIterationsCount = 0;
            this.SimplexIterationsCount = 0;
            this.CuttingPlanesCount = 0;
            this.PreSolveRemovedRows = 0;
            this.PreSolveRemovedColumns = 0;
            this.PreSolveConstraintChanges = 0;
            this.PreSolveBoundChanges = 0;
            this.PreSolveCoefficientChanges = 0;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets a value indicating whether the optimization sense is minimize.
        /// </summary>
        public bool OptimizationSenseIsMinimize { get; }

        /// <summary>
        /// Gets or sets the time stamp of last editing.
        /// </summary>
        public DateTime TimeStampOfLastEditing { get; set; }

        /// <summary>
        /// Gets the time since the last callback.
        /// </summary>
        public double TimeSinceLastCallback =>
            this._immutableTimeSinceLastCallback ?? Math.Max(0, Math.Round((DateTime.Now - this.TimeStampOfLastEditing).TotalMilliseconds));

        /// <summary>
        /// Gets or sets the best objective.
        /// </summary>
        public double BestObjective { get; set; }

        /// <summary>
        /// Gets or sets the best objective bound.
        /// </summary>
        public double BestObjectiveBound { get; set; }

        /// <summary>
        /// Gets or sets the number of feasible solutions.
        /// </summary>
        public double FeasibleSolutionsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of explored nodes.
        /// </summary>
        public double ExploredNodeCount { get; set; }

        /// <summary>
        /// Gets or sets the number of unexplored nodes.
        /// </summary>
        public double UnexploredNodeCount { get; set; }

        /// <summary>
        /// Gets or sets the number of barrier iterations.
        /// </summary>
        public double BarrierIterationsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of simplex iterations.
        /// </summary>
        public double SimplexIterationsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of cutting planes.
        /// </summary>
        public double CuttingPlanesCount { get; set; }

        /// <summary>
        /// Gets the mip gap.
        /// </summary>
        public double MipGap => GurobiUtils.GetMipGap(this.BestObjective, this.BestObjectiveBound);

        /// <summary>
        /// Gets or sets the number of removed rows in pre solve.
        /// </summary>
        public double PreSolveRemovedRows { get; set; }

        /// <summary>
        /// Gets or sets the number of removed columns in pre solve.
        /// </summary>
        public double PreSolveRemovedColumns { get; set; }

        /// <summary>
        /// Gets or sets the number of changed constraints in pre solve.
        /// </summary>
        public double PreSolveConstraintChanges { get; set; }

        /// <summary>
        /// Gets or sets the number of changed bounds in pre solve.
        /// </summary>
        public double PreSolveBoundChanges { get; set; }

        /// <summary>
        /// Gets or sets the number of changed coefficients in pre solve.
        /// </summary>
        public double PreSolveCoefficientChanges { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Copies the current <see cref="GurobiRuntimeFeatures"/> and fixes the TimeSinceLastCallback.
        /// </summary>
        /// <returns>The copy.</returns>
        public GurobiRuntimeFeatures Copy()
        {
            var newGurobiRuntimeFeatures = new GurobiRuntimeFeatures(this.TimeStampOfLastEditing, this.OptimizationSenseIsMinimize)
                                               {
                                                   _immutableTimeSinceLastCallback = this.TimeSinceLastCallback,
                                                   BestObjective = this.BestObjective,
                                                   BestObjectiveBound = this.BestObjectiveBound,
                                                   FeasibleSolutionsCount = this.FeasibleSolutionsCount,
                                                   ExploredNodeCount = this.ExploredNodeCount,
                                                   UnexploredNodeCount = this.UnexploredNodeCount,
                                                   BarrierIterationsCount = this.BarrierIterationsCount,
                                                   SimplexIterationsCount = this.SimplexIterationsCount,
                                                   CuttingPlanesCount = this.CuttingPlanesCount,
                                                   PreSolveRemovedRows = this.PreSolveRemovedRows,
                                                   PreSolveRemovedColumns = this.PreSolveRemovedColumns,
                                                   PreSolveConstraintChanges = this.PreSolveConstraintChanges,
                                                   PreSolveBoundChanges = this.PreSolveBoundChanges,
                                                   PreSolveCoefficientChanges = this.PreSolveCoefficientChanges,
                                               };
            return newGurobiRuntimeFeatures;
        }

        #endregion
    }
}