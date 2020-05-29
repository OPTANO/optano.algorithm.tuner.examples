#region Copyright (c) OPTANO GmbH

// ////////////////////////////////////////////////////////////////////////////////
// 
//        OPTANO GmbH Source Code
//        Copyright (c) 2010-2020 OPTANO GmbH
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
        ///     Creates a cancellable task that runs Gurobi on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">
        ///     Token that is regurlarly checked for cancellation.
        ///     If cancellation is detected, the task will be stopped.
        /// </param>
        /// <returns>
        ///     A task that returns the run's runtime, gap, feasibility and completion status onto return.
        /// </returns>
        public Task<GurobiResult> Run(InstanceSeedFile instance, CancellationToken cancellationToken)
        {
            // Check if the runner has already been disposed.
            if (this._hasAlreadyBeenDisposed)
            {
                throw new ObjectDisposedException("GurobiRunner", "Called Run on a disposed GurobiRunner.");
            }

            // Continue if it hasn't.
            var solveTask = Task.Run(
                () =>
                    {
                        // Prepare Gurobi model: Use configured _environment and the given instance,
                        // then add a callback for cancellation.
                        var instanceFile = new FileInfo(instance.Path);
                        if (!File.Exists(instanceFile.FullName))
                        {
                            throw new Exception(string.Format("Instance {0} not found!", instance.Path));
                        }

                        LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Setting MIPGap to {this._runnerConfiguration.TerminationMipGap}");
                        this._environment.MIPGap = this._runnerConfiguration.TerminationMipGap;
                        LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Current Seed: {instance.Seed}");
                        this._environment.Seed = instance.Seed;
                        this._environment.TimeLimit = this._runnerConfiguration.CpuTimeout.TotalSeconds;

                        var fileName = Path.GetFileNameWithoutExtension(instance.Path);
                        if (!Directory.Exists("GurobiLog"))
                        {
                            Directory.CreateDirectory("GurobiLog");
                        }

                        this._environment.LogFile = $"GurobiLog/GurobiRunner_{DateTime.Now:yy-MM-dd_HH-mm-ss-ffff}_" + fileName + ".log";

                        var model = new GRBModel(this._environment, instance.Path) { ModelName = instanceFile.Name };
                        var mstfileName = instance.Path.Substring(0, instance.Path.Length - instanceFile.Extension.Length) + ".mst";
                        if (File.Exists(mstfileName))
                        {
                            model.Read(mstfileName);
                        }

                        model.SetCallback(new GurobiCallback(cancellationToken));

                        // Optimize. This step may be aborted in the callback.
                        model.Optimize();
                        var result = this.CreateGurobiResult(model);
                        // Before returning, dispose of Gurobi model.
                        model.Dispose();
                        return result;
                    },
                cancellationToken);

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
            var isRunInterrupted = this.GetIsRunInterrupted(model);
            var result = new GurobiResult(
                model.MIPGap,
                // Gurobi sometimes reports a wrong (i.e. too small) runtime when solve is user-interrupted. Use the actual configured timeout instead of the measured.
                isRunInterrupted ? this._runnerConfiguration.CpuTimeout : TimeSpan.FromSeconds(model.Runtime),
                isRunInterrupted,
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
