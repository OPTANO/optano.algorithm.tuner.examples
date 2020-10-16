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

namespace Optano.Algorithm.Tuner.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Runs a predefined command on the command line by adding parameters and the path to an instance file and
    /// measures the time this process runs.
    /// </summary>
    public class TimeMeasuringExecutor : CommandExecutorBase<RuntimeResult>
    {
        #region Fields

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly TimeSpan _timeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeMeasuringExecutor" /> class.
        /// </summary>
        /// <param name="parameters">Parameters for the target algorithm.</param>
        /// <param name="basicCommand">The basic command to the target algorithm as it should be executed by the
        /// command line. The path to the instance file and the parameters will be set by replacing
        /// <see cref="CommandExecutorBase{TResult}.InstanceReplacement" /> and
        /// <see cref="CommandExecutorBase{TResult}.ParameterReplacement" />.</param>
        /// <param name="timeout">The timeout.</param>
        public TimeMeasuringExecutor(Dictionary<string, IAllele> parameters, string basicCommand, TimeSpan timeout)
            : base(parameters, basicCommand)
        {
            this._timeout = timeout;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a cancellable task that runs the <see cref="CommandExecutorBase{TResult}.Command"/> on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">Token that is regurlarly checked for cancellation.
        /// If cancellation is detected, the task will be stopped.</param>
        /// <returns>A task that returns the run's runtime on completion.</returns>
        public override Task<RuntimeResult> Run(InstanceFile instance, CancellationToken cancellationToken)
        {
            // Define process to target algorithm from command line.
            var processInfo = this.BuildProcessStartInfo(instance);

            return Task.Run(
                function: () =>
                    {
                        var timer = new Stopwatch();
                        timer.Start();
                        // Start process and make sure it's cancelled if the cancellationToken is cancelled.
                        using (var process = Process.Start(processInfo))
                        using (var processRegistration =
                            cancellationToken.Register(() => ProcessUtils.CancelProcess(process)))
                        {
                            // Wait until end of process.
                            process.WaitForExit();

                            // If the process was cancelled, escalate it up.
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            // If the process has inappropriate exit code, clean up resources and return cancelled result.
                            if (process.ExitCode != 0)
                            {
                                return RuntimeResult.CreateCancelledResult(this._timeout);
                            }

                            // If the process was not cancelled, return CPU time as result.
                            timer.Stop();
                            return new RuntimeResult(timer.Elapsed);
                        }
                    },
                cancellationToken: cancellationToken);
        }

        #endregion
    }
}