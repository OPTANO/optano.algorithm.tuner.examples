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
    using System.Threading;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Gurobi.GurobiAdapterFeatures;

    /// <summary>
    ///     Responsible for defining what Gurobi should do on callbacks.
    /// </summary>
    internal class GurobiCallback : GRBCallback
    {
        #region Fields

        /// <summary>
        /// The cancellation token issued by OPTANO Algorithm Tuner.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Boolean indicating, if data should be recorded.
        /// </summary>
        private readonly bool _recordData;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GurobiCallback" /> class.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token issued by OPTANO Algorithm Tuner.</param>
        /// <param name="recordData"> Boolean indicating, if data should be recorded.</param>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="optimizationSenseIsMinimize">A value indicating whether the optimization sense is minimize.</param>
        public GurobiCallback(
            CancellationToken cancellationToken,
            bool recordData,
            DateTime timeStamp,
            bool optimizationSenseIsMinimize)
        {
            this._cancellationToken = cancellationToken;
            this._recordData = recordData;

            if (this._recordData)
            {
                this.CurrentRuntimeFeatures = new GurobiRuntimeFeatures(timeStamp, optimizationSenseIsMinimize);
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the current <see cref="GurobiRuntimeFeatures"/>.
        /// </summary>
        public GurobiRuntimeFeatures CurrentRuntimeFeatures { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Custom callback implementation that aborts the optimization when the <see cref="_cancellationToken" /> requests it.
        /// </summary>
        protected override void Callback()
        {
            if (this._cancellationToken.IsCancellationRequested)
            {
                this.Abort();
            }

            if (this._recordData)
            {
                this.UpdateCurrentGurobiFeatures();
            }
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/>.
        /// </summary>
        private void UpdateCurrentGurobiFeatures()
        {
            switch (this.@where)
            {
                case GRB.Callback.PRESOLVE:
                    this.UpdateCurrentGurobiFeaturesInPresolvePhase();
                    break;
                case GRB.Callback.SIMPLEX:
                    this.UpdateCurrentGurobiFeaturesInSimplexPhase();
                    break;
                case GRB.Callback.MIP:
                    this.UpdateCurrentGurobiFeaturesInMipPhase();
                    break;
                case GRB.Callback.MIPSOL:
                    this.UpdateCurrentGurobiFeaturesInMipSolPhase();
                    break;
                case GRB.Callback.MIPNODE:
                    this.UpdateCurrentGurobiFeaturesInMipNodePhase();
                    break;
                case GRB.Callback.BARRIER:
                    this.UpdateCurrentGurobiFeaturesInBarrierPhase();
                    break;
                default:
                    return;
            }

            this.CurrentRuntimeFeatures.TimeStampOfLastEditing = DateTime.Now;
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/> in presolve phase.
        /// </summary>
        private void UpdateCurrentGurobiFeaturesInPresolvePhase()
        {
            this.CurrentRuntimeFeatures.PreSolveRemovedRows = this.GetCallbackInt(
                GRB.Callback.PRE_ROWDEL,
                this.CurrentRuntimeFeatures.PreSolveRemovedRows);
            this.CurrentRuntimeFeatures.PreSolveRemovedColumns = this.GetCallbackInt(
                GRB.Callback.PRE_COLDEL,
                this.CurrentRuntimeFeatures.PreSolveRemovedColumns);
            this.CurrentRuntimeFeatures.PreSolveConstraintChanges = this.GetCallbackInt(
                GRB.Callback.PRE_SENCHG,
                this.CurrentRuntimeFeatures.PreSolveConstraintChanges);
            this.CurrentRuntimeFeatures.PreSolveBoundChanges = this.GetCallbackInt(
                GRB.Callback.PRE_BNDCHG,
                this.CurrentRuntimeFeatures.PreSolveBoundChanges);
            this.CurrentRuntimeFeatures.PreSolveCoefficientChanges = this.GetCallbackInt(
                GRB.Callback.PRE_COECHG,
                this.CurrentRuntimeFeatures.PreSolveCoefficientChanges);
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/> in simplex phase.
        /// </summary>
        private void UpdateCurrentGurobiFeaturesInSimplexPhase()
        {
            this.CurrentRuntimeFeatures.SimplexIterationsCount = this.GetCallbackDouble(
                GRB.Callback.SPX_ITRCNT,
                this.CurrentRuntimeFeatures.SimplexIterationsCount);
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/> in MIP phase.
        /// </summary>
        private void UpdateCurrentGurobiFeaturesInMipPhase()
        {
            this.CurrentRuntimeFeatures.BestObjective = this.GetCallbackDouble(GRB.Callback.MIP_OBJBST, this.CurrentRuntimeFeatures.BestObjective);
            this.CurrentRuntimeFeatures.BestObjectiveBound = this.GetCallbackDouble(
                GRB.Callback.MIP_OBJBND,
                this.CurrentRuntimeFeatures.BestObjectiveBound);
            this.CurrentRuntimeFeatures.FeasibleSolutionsCount = this.GetCallbackInt(
                GRB.Callback.MIP_SOLCNT,
                this.CurrentRuntimeFeatures.FeasibleSolutionsCount);
            this.CurrentRuntimeFeatures.ExploredNodeCount = this.GetCallbackDouble(
                GRB.Callback.MIP_NODCNT,
                this.CurrentRuntimeFeatures.ExploredNodeCount);
            this.CurrentRuntimeFeatures.UnexploredNodeCount = this.GetCallbackDouble(
                GRB.Callback.MIP_NODLFT,
                this.CurrentRuntimeFeatures.UnexploredNodeCount);
            this.CurrentRuntimeFeatures.SimplexIterationsCount = this.GetCallbackDouble(
                GRB.Callback.MIP_ITRCNT,
                this.CurrentRuntimeFeatures.SimplexIterationsCount);
            this.CurrentRuntimeFeatures.CuttingPlanesCount = this.GetCallbackInt(
                GRB.Callback.MIP_CUTCNT,
                this.CurrentRuntimeFeatures.CuttingPlanesCount);
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/> in MIP_SOL phase.
        /// </summary>
        private void UpdateCurrentGurobiFeaturesInMipSolPhase()
        {
            this.CurrentRuntimeFeatures.BestObjective = this.GetCallbackDouble(GRB.Callback.MIPSOL_OBJBST, this.CurrentRuntimeFeatures.BestObjective);
            this.CurrentRuntimeFeatures.BestObjectiveBound = this.GetCallbackDouble(
                GRB.Callback.MIPSOL_OBJBND,
                this.CurrentRuntimeFeatures.BestObjectiveBound);
            this.CurrentRuntimeFeatures.FeasibleSolutionsCount = this.GetCallbackInt(
                GRB.Callback.MIPSOL_SOLCNT,
                this.CurrentRuntimeFeatures.FeasibleSolutionsCount);
            this.CurrentRuntimeFeatures.ExploredNodeCount = this.GetCallbackDouble(
                GRB.Callback.MIPSOL_NODCNT,
                this.CurrentRuntimeFeatures.ExploredNodeCount);
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/> in MIP_NODE phase.
        /// </summary>
        private void UpdateCurrentGurobiFeaturesInMipNodePhase()
        {
            this.CurrentRuntimeFeatures.BestObjective = this.GetCallbackDouble(
                GRB.Callback.MIPNODE_OBJBST,
                this.CurrentRuntimeFeatures.BestObjective);
            this.CurrentRuntimeFeatures.BestObjectiveBound = this.GetCallbackDouble(
                GRB.Callback.MIPNODE_OBJBND,
                this.CurrentRuntimeFeatures.BestObjectiveBound);
            this.CurrentRuntimeFeatures.FeasibleSolutionsCount = this.GetCallbackInt(
                GRB.Callback.MIPNODE_SOLCNT,
                this.CurrentRuntimeFeatures.FeasibleSolutionsCount);
            this.CurrentRuntimeFeatures.ExploredNodeCount = this.GetCallbackDouble(
                GRB.Callback.MIPNODE_NODCNT,
                this.CurrentRuntimeFeatures.ExploredNodeCount);
        }

        /// <summary>
        /// Updates the <see cref="CurrentRuntimeFeatures"/> in barrier phase.
        /// </summary>
        private void UpdateCurrentGurobiFeaturesInBarrierPhase()
        {
            this.CurrentRuntimeFeatures.BarrierIterationsCount = this.GetCallbackInt(
                GRB.Callback.BARRIER_ITRCNT,
                this.CurrentRuntimeFeatures.BarrierIterationsCount);
        }

        /// <summary>
        /// Gets the callback value of an int gurobi information in a try-catch block.
        /// </summary>
        /// <param name="callbackCode">The callback code.</param>
        /// <param name="fallback">The fallback value.</param>
        /// <returns>The callback value as double.</returns>
        private double GetCallbackInt(int callbackCode, double fallback)
        {
            double doubleValue;
            try
            {
                doubleValue = this.GetIntInfo(callbackCode);
            }
            catch
            {
                doubleValue = fallback;
            }

            return doubleValue;
        }

        /// <summary>
        /// Gets the callback value of a double gurobi information in a try-catch block.
        /// </summary>
        /// <param name="callbackCode">The callback code.</param>
        /// <param name="fallback">The fallback value.</param>
        /// <returns>The callback value as double.</returns>
        private double GetCallbackDouble(int callbackCode, double fallback)
        {
            double doubleValue;
            try
            {
                doubleValue = this.GetDoubleInfo(callbackCode);
            }
            catch
            {
                doubleValue = fallback;
            }

            return doubleValue;
        }

        #endregion
    }
}