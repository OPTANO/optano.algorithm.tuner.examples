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
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.GrayBox;
    using Optano.Algorithm.Tuner.GrayBox.DataRecordTypes;
    using Optano.Algorithm.Tuner.Gurobi.GurobiAdapterFeatures;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    ///     Runs the MIP solver Gurobi.
    /// </summary>
    public class GurobiRunner : IGrayBoxTargetAlgorithm<InstanceSeedFile, GurobiResult>, IDisposable
    {
        #region Fields

        /// <summary>
        /// Gurobi _environment object that has been pre-configured using a genome.
        /// </summary>
        private readonly GRBEnv _environment;

        /// <summary>
        /// The runner configuration.
        /// </summary>
        private readonly GurobiRunnerConfiguration _runnerConfiguration;

        /// <summary>
        /// The tuner configuration.
        /// </summary>
        private readonly AlgorithmTunerConfiguration _tunerConfiguration;

        /// <summary>
        ///     A flag indicating whether the object has been disposed already.
        /// </summary>
        private bool _hasAlreadyBeenDisposed;

        /// <summary>
        /// The Gurobi callback.
        /// </summary>
        private GurobiCallback _grbCallback;

        /// <summary>
        /// The current target algorithm status.
        /// </summary>
        private TargetAlgorithmStatus _targetAlgorithmStatus;

        /// <summary>
        /// The timer to record data.
        /// </summary>
        private Timer _recordTimer;

        /// <summary>
        /// The inner cancellation token source.
        /// </summary>
        private CancellationTokenSource _innerCancellationTokenSource;

        /// <summary>
        /// The last <see cref="GurobiRuntimeFeatures"/>.
        /// </summary>
        private GurobiRuntimeFeatures _lastRuntimeFeatures;

        /// <summary>
        /// The start time stamp.
        /// </summary>
        private DateTime _startTimeStamp;

        /// <summary>
        /// The <see cref="_instanceFeatures"/>.
        /// </summary>
        private GurobiInstanceFeatures _instanceFeatures;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GurobiRunner" /> class.
        /// </summary>
        /// <param name="gurobiEnvironment">The _environment to use for all runs.</param>
        /// <param name="runnerConfiguration">The <see cref="GurobiRunnerConfiguration"/>.</param>
        /// <param name="tunerConfiguration">The <see cref="AlgorithmTunerConfiguration"/>.</param>
        public GurobiRunner(GRBEnv gurobiEnvironment, GurobiRunnerConfiguration runnerConfiguration, AlgorithmTunerConfiguration tunerConfiguration)
        {
            this._environment = gurobiEnvironment;
            this._runnerConfiguration = runnerConfiguration;
            this._tunerConfiguration = tunerConfiguration;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="GurobiRunner" /> class.
        ///     Needed to make sure licenses are released.
        /// </summary>
        ~GurobiRunner()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Events

        /// <inheritdoc />
        public event EventHandler<AdapterDataRecord<GurobiResult>> OnNewDataRecord;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Releases the resources associated with this runner.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Creates a task that runs Gurobi on the given instance.
        /// </summary>
        /// <param name="instance">
        /// Instance to run on.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token given to the task. NOTE: In this implementation, we ignore this cancellation token, since Gurobi uses its own cancellation token in the callback.
        /// </param>
        /// <returns>
        /// A task that returns the run's runtime, gap, feasibility and completion status onto return.
        /// </returns>
        public Task<GurobiResult> Run(InstanceSeedFile instance, CancellationToken cancellationToken)
        {
            // Check if the runner has already been disposed.
            if (this._hasAlreadyBeenDisposed)
            {
                throw new ObjectDisposedException("GurobiRunner", "Called Run on a disposed GurobiRunner.");
            }

            // Continue if it hasn't.
            // ReSharper disable once MethodSupportsCancellation
            var solveTask = Task.Run(
                () =>
                    {
                        this._recordTimer = null;
                        try
                        {
                            // Prepare Gurobi model: Use configured _environment and the given instance,
                            // then add a cancellation token source and the Gurobi callback for cancellation.
                            var instanceFile = new FileInfo(instance.Path);
                            if (!File.Exists(instanceFile.FullName))
                            {
                                throw new FileNotFoundException($"Instance {instanceFile.FullName} not found!");
                            }

                            LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Setting MIPGap to {this._runnerConfiguration.TerminationMipGap}");
                            this._environment.MIPGap = this._runnerConfiguration.TerminationMipGap;
                            LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Current Seed: {instance.Seed}");
                            this._environment.Seed = instance.Seed;
                            this._environment.TimeLimit = this._runnerConfiguration.CpuTimeout.TotalSeconds;

                            if (!Directory.Exists("GurobiLog"))
                            {
                                Directory.CreateDirectory("GurobiLog");
                            }

                            var instanceFileNameWithoutExtension = GurobiUtils.GetFileNameWithoutGurobiExtension(instanceFile);

                            this._environment.LogFile = "GurobiLog" + Path.DirectorySeparatorChar
                                                                    + $"GurobiRunner_{DateTime.Now:yy-MM-dd_HH-mm-ss-ffff}_"
                                                                    + instanceFileNameWithoutExtension + $"_{Guid.NewGuid()}.log";

                            var model = new GRBModel(this._environment, instanceFile.FullName) { ModelName = instanceFileNameWithoutExtension };

                            if (GurobiRunner.TryToGetMstFile(instanceFile.DirectoryName, instanceFileNameWithoutExtension, out var mstFileFullName))
                            {
                                model.Read(mstFileFullName);
                            }

                            this._startTimeStamp = DateTime.Now;
                            this._targetAlgorithmStatus = TargetAlgorithmStatus.Running;

                            this._innerCancellationTokenSource = new CancellationTokenSource(this._runnerConfiguration.CpuTimeout);

                            this._grbCallback = new GurobiCallback(
                                this._innerCancellationTokenSource.Token,
                                this._tunerConfiguration.EnableDataRecording,
                                this._startTimeStamp);
                            model.SetCallback(this._grbCallback);

                            if (this._tunerConfiguration.EnableDataRecording)
                            {
                                this._lastRuntimeFeatures = null;
                                this.SetGurobiInstanceFeatures(model);

                                // Start record timer.
                                var autoResetEvent = new AutoResetEvent(false);
                                this._recordTimer = new Timer(
                                    (timerCallback) =>
                                        {
                                            var currentTimeStamp = DateTime.Now;
                                            if (currentTimeStamp - this._startTimeStamp < this._runnerConfiguration.CpuTimeout)
                                            {
                                                // Write current line to data log.
                                                var currentAdapterData = this.CreateCurrentAdapterDataRecord(currentTimeStamp);
                                                this.OnNewDataRecord?.Invoke(this, currentAdapterData);
                                            }
                                        },
                                    autoResetEvent,
                                    TimeSpan.FromSeconds(0),
                                    this._tunerConfiguration.DataRecordUpdateInterval);
                            }

                            // Optimize. This step may be aborted in the callback.
                            model.Optimize();

                            if (this._targetAlgorithmStatus != TargetAlgorithmStatus.CancelledByGrayBox)
                            {
                                this._recordTimer?.Dispose();
                                this._targetAlgorithmStatus = this.GetIsRunInterrupted(model)
                                                                  ? TargetAlgorithmStatus.CancelledByTimeout
                                                                  : TargetAlgorithmStatus.Finished;
                            }

                            var finalTimeStamp = DateTime.Now;
                            var result = this.CreateGurobiResult(finalTimeStamp, model);

                            if (this._tunerConfiguration.EnableDataRecording)
                            {
                                // Write last line to data log.
                                var lastAdapterData = this.CreateFinalAdapterDataRecord(finalTimeStamp, model, result);
                                this.OnNewDataRecord?.Invoke(this, lastAdapterData);
                            }

                            // Before returning, dispose of Gurobi model.
                            model.Dispose();

                            return result;
                        }
                        finally
                        {
                            this._recordTimer?.Dispose();
                        }
                    });

            return solveTask;
        }

        /// <inheritdoc/>
        public void CancelByGrayBox()
        {
            this._targetAlgorithmStatus = TargetAlgorithmStatus.CancelledByGrayBox;
            this._recordTimer?.Dispose();
            this._innerCancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Creates the gurobi result.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="model">The <see cref="GRBModel"/>.</param>
        /// <returns>The result.</returns>
        public GurobiResult CreateGurobiResult(DateTime timeStamp, GRBModel model)
        {
            // Return result even if the task was cancelled as the MIP gap might still be small.
            var result = new GurobiResult(
                this.GetValueWithTryCatch(() => model.MIPGap, GRB.INFINITY),
                this.GetFinalRuntime(timeStamp),
                this._targetAlgorithmStatus,
                this.HasFoundSolution(model));
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Releases the resources associated with <see cref="_environment" />.
        /// </summary>
        /// <param name="disposing">Whether this got called by <see cref="Dispose()" /> or the class's finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._hasAlreadyBeenDisposed)
            {
                this._environment.Dispose();
                this._hasAlreadyBeenDisposed = true;
            }
        }

        /// <summary>
        /// Tries to get the full file name of a corresponding mst file, if existing.
        /// </summary>
        /// <param name="instanceDirectoryName">The full directory name of the current instance file.</param>
        /// <param name="instanceFileNameWithoutExtension">The file name without extension of the current instance file.</param>
        /// <param name="mstFileFullName">The full file name of the corresponding mst file.</param>
        /// <returns>True, if a corresponding mst file exists.</returns>
        private static bool TryToGetMstFile(string instanceDirectoryName, string instanceFileNameWithoutExtension, out string mstFileFullName)
        {
            mstFileFullName = GurobiUtils.ListOfValidFileCompressionExtensions.Select(
                    compressionExtension => instanceDirectoryName + Path.DirectorySeparatorChar + instanceFileNameWithoutExtension + ".mst"
                                            + compressionExtension)
                .FirstOrDefault(File.Exists);

            return mstFileFullName != null;
        }

        /// <summary>
        /// Gets the value of a method, which returns a value, in a try catch block.
        /// </summary>
        /// <typeparam name="TReturnValue">The return type.</typeparam>
        /// <param name="getValue">The desired method.</param>
        /// <param name="fallback">The fallback value.</param>
        /// <returns>The desired value.</returns>
        private TReturnValue GetValueWithTryCatch<TReturnValue>(Func<TReturnValue> getValue, TReturnValue fallback)
        {
            try
            {
                return getValue();
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// Gets the final runtime.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns>The final runtime.</returns>
        private TimeSpan GetFinalRuntime(DateTime timeStamp)
        {
            return this._targetAlgorithmStatus != TargetAlgorithmStatus.Finished
                       ? this._runnerConfiguration.CpuTimeout
                       : this.GetCurrentRuntime(timeStamp);
        }

        /// <summary>
        /// Gets the current runtime.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns>The current runtime.</returns>
        private TimeSpan GetCurrentRuntime(DateTime timeStamp)
        {
            var currentWallClockTime = timeStamp - this._startTimeStamp;
            return currentWallClockTime >= this._runnerConfiguration.CpuTimeout
                       ? this._runnerConfiguration.CpuTimeout
                       : currentWallClockTime;
        }

        /// <summary>
        /// Checks, if the run is interrupted.
        /// </summary>
        /// <param name="model">The <see cref="GRBModel"/>.</param>
        /// <returns>True, if run is interrupted.</returns>
        private bool GetIsRunInterrupted(GRBModel model)
        {
            return model.Status == GRB.Status.INTERRUPTED
                   || model.Status == GRB.Status.TIME_LIMIT
                   || model.Status == GRB.Status.NUMERIC
                   || model.Status == GRB.Status.INPROGRESS
                   || model.Status == GRB.Status.LOADED;
        }

        /// <summary>
        /// Checks whether the current Gurobi model has found a feasible solution.
        /// </summary>
        /// <param name="model">The <see cref="GRBModel"/>.</param>
        /// <returns>Whether the model has found a feasible solution.</returns>
        private bool HasFoundSolution(GRBModel model)
        {
            // Handling model.status is an overkill here, so we use the following hack:
            try
            {
                // Check if the first variable is set.
                var firstVariable = model.GetVars()[0];
                firstVariable.Get(GRB.DoubleAttr.X);
                // If it is, we have a solution.
                return true;
            }
            catch (GRBException)
            {
                // But if it throws an exception, we don't.
                return false;
            }
        }

        /// <summary>
        /// Sets the current <see cref="GurobiInstanceFeatures"/>.
        /// </summary>
        /// <param name="model">The <see cref="GRBModel"/>.</param>
        private void SetGurobiInstanceFeatures(GRBModel model)
        {
            this._instanceFeatures = new GurobiInstanceFeatures()
                                         {
                                             NumberOfBinaryVariables = this.GetValueWithTryCatch(() => model.NumBinVars, 0),
                                             NumberOfGeneralConstraints = this.GetValueWithTryCatch(() => model.NumGenConstrs, 0),
                                             NumberOfIntegerVariables = this.GetValueWithTryCatch(() => model.NumIntVars, 0),
                                             NumberOfLinearConstraints = this.GetValueWithTryCatch(() => model.NumConstrs, 0),
                                             NumberOfNonZeroCoefficients = this.GetValueWithTryCatch(() => model.NumNZs, 0),
                                             NumberOfNonZeroQuadraticObjectiveTerms =
                                                 this.GetValueWithTryCatch(() => model.NumQNZs, 0),
                                             NumberOfNonZeroTermsInQuadraticConstraints =
                                                 this.GetValueWithTryCatch(() => model.NumQCNZs, 0),
                                             NumberOfQuadraticConstraints = this.GetValueWithTryCatch(() => model.NumQConstrs, 0),
                                             NumberOfSosConstraints = this.GetValueWithTryCatch(() => model.NumSOS, 0),
                                             NumberOfVariables = this.GetValueWithTryCatch(() => model.NumVars, 0),
                                             NumberOfVariablesWithPiecewiseLinearObjectiveFunctions = this.GetValueWithTryCatch(
                                                 () => model.NumPWLObjVars,
                                                 0),
                                         };
        }

        /// <summary>
        /// Creates the current <see cref="AdapterDataRecord{GurobiResult}"/>.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <returns>The current <see cref="AdapterDataRecord{GurobiResult}"/>.</returns>
        private AdapterDataRecord<GurobiResult> CreateCurrentAdapterDataRecord(DateTime timeStamp)
        {
            if (this._instanceFeatures == null)
            {
                throw new ArgumentNullException(nameof(this._instanceFeatures));
            }

            var currentRuntimeFeatures = this.GetCurrentGurobiRuntimeFeatures();
            var lastRuntimeFeatures = this._lastRuntimeFeatures ?? currentRuntimeFeatures;
            var adapterFeatures = GurobiUtils.ComposeAdapterFeatures(currentRuntimeFeatures, lastRuntimeFeatures, this._instanceFeatures);
            var adapterFeaturesHeader = GurobiUtils.ComposeAdapterFeaturesHeader(currentRuntimeFeatures, lastRuntimeFeatures, this._instanceFeatures);
            this._lastRuntimeFeatures = currentRuntimeFeatures.Copy();

            return new AdapterDataRecord<GurobiResult>(
                "Gurobi901",
                TargetAlgorithmStatus.Running,
                // The cpu time is not recordable, because all parallel Gurobi runs are started in the same process in this implementation.
                TimeSpan.FromSeconds(0),
                this.GetCurrentRuntime(timeStamp),
                timeStamp,
                adapterFeaturesHeader,
                adapterFeatures,
                new GurobiResult(
                    currentRuntimeFeatures.MipGap,
                    this._runnerConfiguration.CpuTimeout,
                    TargetAlgorithmStatus.CancelledByGrayBox,
                    currentRuntimeFeatures.FeasibleSolutionsCount > 0));
        }

        /// <summary>
        /// Creates the final <see cref="AdapterDataRecord{GurobiResult}"/>.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="model">The <see cref="GRBModel"/>.</param>
        /// <param name="result">The <see cref="GurobiResult"/>.</param>
        /// <returns>The final <see cref="AdapterDataRecord{GurobiResult}"/>.</returns>
        private AdapterDataRecord<GurobiResult> CreateFinalAdapterDataRecord(DateTime timeStamp, GRBModel model, GurobiResult result)
        {
            if (this._instanceFeatures == null)
            {
                throw new ArgumentNullException(nameof(this._instanceFeatures));
            }

            var finalRuntimeFeatures = this.GetFinalGurobiRuntimeFeatures(model, result);
            var lastRuntimeFeatures = this._lastRuntimeFeatures ?? finalRuntimeFeatures;
            var adapterFeatures = GurobiUtils.ComposeAdapterFeatures(finalRuntimeFeatures, lastRuntimeFeatures, this._instanceFeatures);
            var adapterFeaturesHeader = GurobiUtils.ComposeAdapterFeaturesHeader(finalRuntimeFeatures, lastRuntimeFeatures, this._instanceFeatures);
            this._lastRuntimeFeatures = finalRuntimeFeatures.Copy();

            return new AdapterDataRecord<GurobiResult>(
                "Gurobi901",
                result.TargetAlgorithmStatus,
                // The cpu time is not recordable, because all parallel Gurobi runs are started in the same process in this implementation.
                TimeSpan.FromSeconds(0),
                this.GetCurrentRuntime(timeStamp),
                timeStamp,
                adapterFeaturesHeader,
                adapterFeatures,
                result);
        }

        /// <summary>
        /// Gets the current <see cref="GurobiRuntimeFeatures"/>.
        /// </summary>
        /// <returns>The current <see cref="GurobiRuntimeFeatures"/>.</returns>
        private GurobiRuntimeFeatures GetCurrentGurobiRuntimeFeatures()
        {
            if (this._grbCallback.CurrentRuntimeFeatures == null)
            {
                throw new ArgumentNullException(nameof(this._grbCallback.CurrentRuntimeFeatures));
            }

            return this._grbCallback.CurrentRuntimeFeatures.Copy();
        }

        /// <summary>
        /// Gets the final <see cref="GurobiRuntimeFeatures"/>.
        /// </summary>
        /// <param name="model">The <see cref="GRBModel"/>.</param>
        /// <param name="result">The <see cref="GurobiResult"/>.</param>
        /// <returns>The final <see cref="GurobiRuntimeFeatures"/>.</returns>
        private GurobiRuntimeFeatures GetFinalGurobiRuntimeFeatures(GRBModel model, GurobiResult result)
        {
            // Get current runtime features.
            var runtimeFeatures = this.GetCurrentGurobiRuntimeFeatures();

            // Update all feature values, provided by GurobiResult.
            runtimeFeatures.MipGap = result.Gap;

            // Update all feature values, provided by GRBModel.
            runtimeFeatures.BarrierIterationsCount = this.GetValueWithTryCatch(
                () => model.BarIterCount,
                runtimeFeatures.BarrierIterationsCount);
            runtimeFeatures.BestObjective = this.GetValueWithTryCatch(() => model.ObjVal, runtimeFeatures.BestObjective);
            runtimeFeatures.BestObjectiveBound = this.GetValueWithTryCatch(() => model.ObjBound, runtimeFeatures.BestObjectiveBound);
            runtimeFeatures.ExploredNodeCount = this.GetValueWithTryCatch(() => model.NodeCount, runtimeFeatures.ExploredNodeCount);
            runtimeFeatures.SimplexIterationsCount = this.GetValueWithTryCatch(() => model.IterCount, runtimeFeatures.SimplexIterationsCount);

            // Since GRBModel does not provide the number of unexplored nodes, use last entry or 0, depending on the run status.
            runtimeFeatures.UnexploredNodeCount =
                result.TargetAlgorithmStatus != TargetAlgorithmStatus.Finished ? runtimeFeatures.UnexploredNodeCount : 0;

            // Since GRBModel.SolCount just reports the number of stored solutions, use maximum between last entry and GRBModel.SolCount.
            var feasibleSolutionsCountFromModel = this.GetValueWithTryCatch(() => model.SolCount, runtimeFeatures.FeasibleSolutionsCount);
            runtimeFeatures.FeasibleSolutionsCount = Math.Max(feasibleSolutionsCountFromModel, runtimeFeatures.FeasibleSolutionsCount);

            return runtimeFeatures;
        }

        #endregion
    }
}