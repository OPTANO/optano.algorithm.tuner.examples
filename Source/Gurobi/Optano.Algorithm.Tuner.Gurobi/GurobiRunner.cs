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

    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    ///     Runs the MIP solver Gurobi.
    /// </summary>
    public class GurobiRunner : ITargetAlgorithm<InstanceSeedFile, GurobiResult>, IDisposable
    {
        #region Fields

        /// <summary>
        ///     Gurobi _environment object that has been pre-configured using a genome.
        /// </summary>
        private readonly GRBEnv _environment;

        /// <summary>
        /// The runner configuration.
        /// </summary>
        private readonly GurobiRunnerConfiguration _runnerConfiguration;

        /// <summary>
        ///     A flag indicating whether the object has been disposed already.
        /// </summary>
        private bool _hasAlreadyBeenDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GurobiRunner" /> class.
        /// </summary>
        /// <param name="gurobiEnvironment">The _environment to use for all runs.</param>
        /// <param name="runnerConfiguration">The configuration for the <see cref="GurobiRunner"/>.</param>
        public GurobiRunner(GRBEnv gurobiEnvironment, GurobiRunnerConfiguration runnerConfiguration)
        {
            this._environment = gurobiEnvironment;
            this._runnerConfiguration = runnerConfiguration;
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

        #region Properties

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        private DateTime StartTime { get; set; }

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

                        this._environment.LogFile = "GurobiLog" + Path.DirectorySeparatorChar + $"GurobiRunner_{DateTime.Now:yy-MM-dd_HH-mm-ss-ffff}_"
                                                    + instanceFileNameWithoutExtension + $"_{Guid.NewGuid()}.log";

                        var model = new GRBModel(this._environment, instanceFile.FullName) { ModelName = instanceFileNameWithoutExtension };

                        if (GurobiRunner.TryToGetMstFile(instanceFile.DirectoryName, instanceFileNameWithoutExtension, out var mstFileFullName))
                        {
                            model.Read(mstFileFullName);
                        }

                        var cancellationTokenSource = new CancellationTokenSource(this._runnerConfiguration.CpuTimeout);

                        model.SetCallback(new GurobiCallback(cancellationTokenSource.Token));

                        this.StartTime = DateTime.Now;

                        // Optimize. This step may be aborted in the callback.
                        model.Optimize();
                        var result = this.CreateGurobiResult(model);
                        // Before returning, dispose of Gurobi model.
                        model.Dispose();
                        return result;
                    });

            return solveTask;
        }

        /// <summary>
        /// Creates the gurobi result.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The result.</returns>
        public GurobiResult CreateGurobiResult(GRBModel model)
        {
            // Return result even if the task was cancelled as the MIP gap might still be small.
            var result = new GurobiResult(
                this.GetValueWithTryCatch(() => model.MIPGap, GRB.INFINITY),
                this.GetCorrectRuntime(model),
                this.GetIsRunInterrupted(model),
                this.HasFoundSolution(model));
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Releases the resources associated with <see cref="_environment" />.
        /// </summary>
        /// <param name="disposing">Whether this got called by <see cref="GurobiRunner.Dispose()" /> or the class's finalizer.</param>
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
        /// Since Gurobi might not stop directly after its timeout, we need to return the corrected runtime of Gurobi.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The corrected runtime.</returns>
        private TimeSpan GetCorrectRuntime(GRBModel model)
        {
            var wallClockTime = DateTime.Now - this.StartTime;
            return (this.GetIsRunInterrupted(model) || this._runnerConfiguration.CpuTimeout < wallClockTime)
                       ? this._runnerConfiguration.CpuTimeout
                       : wallClockTime;
        }

        /// <summary>
        /// Checks, if the run is interrupted.
        /// </summary>
        /// <param name="model">The model.</param>
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
        ///     Checks whether the given Gurobi model has found a feasible solution.
        /// </summary>
        /// <param name="model">Model to check.</param>
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

        #endregion
    }
}