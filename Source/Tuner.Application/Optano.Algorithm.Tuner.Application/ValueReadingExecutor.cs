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
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Runs a predefined command on the command line by adding parameters and the path to an instance file and
    /// reads the last number the command writes to console before termination.
    /// </summary>
    public class ValueReadingExecutor : CommandExecutorBase<ContinuousResult>
    {
        #region Static Fields

        /// <summary>
        /// A regular expression that matches numbers in different formats, last numbers first.
        /// </summary>
        private static readonly Regex NumberMatcher = new Regex(
            @"[-+]?\d*\.?\d+([eE][-+]?\d+)?",
            RegexOptions.RightToLeft);

        #endregion

        #region Fields

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly TimeSpan _timeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueReadingExecutor" /> class.
        /// </summary>
        /// <param name="parameters">Parameters for the target algorithm.</param>
        /// <param name="basicCommand">The basic command to the target algorithm as it should be executed by the
        /// command line. The path to the instance file and the parameters will be set by replacing
        /// <see cref="CommandExecutorBase{TResult}.InstanceReplacement" /> and
        /// <see cref="CommandExecutorBase{TResult}.ParameterReplacement" />.</param>
        /// <param name="timeout">The timeout.</param>
        public ValueReadingExecutor(Dictionary<string, IAllele> parameters, string basicCommand, TimeSpan timeout)
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
        /// <returns>A task that returns the run's result on completion.</returns>
        public override Task<ContinuousResult> Run(InstanceFile instance, CancellationToken cancellationToken)
        {
            // Define process and redirect standard output to read value to optimize from output.
            var processInfo = this.BuildProcessStartInfo(instance);
            processInfo.RedirectStandardOutput = true;

            return Task.Run(
                function: () =>
                    {
                        // Start process.
                        var timer = new Stopwatch();
                        timer.Start();
                        using (var process = Process.Start(processInfo))
                        using (var processRegistration =
                            cancellationToken.Register(() => ProcessUtils.CancelProcess(process)))
                        {
                            // Wait until end of process.
                            process.WaitForExit();

                            // If the process was cancelled, clean up resources and escalate it up.
                            if (cancellationToken.IsCancellationRequested)
                            {
                                this.CleanUp(process);
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            // If the process has inappropriate exit code, clean up resources and return cancelled result.
                            if (process.ExitCode != 0)
                            {
                                this.CleanUp(process);
                                return ContinuousResult.CreateCancelledResult(this._timeout);
                            }

                            // If the process was not cancelled, find the last value written to console.
                            string output = process.StandardOutput.ReadToEnd();
                            timer.Stop();

                            // If the output does not match to regex, clean up resources and return cancelled result.
                            if (!ValueReadingExecutor.NumberMatcher.IsMatch(output))
                            {
                                this.CleanUp(process);
                                return ContinuousResult.CreateCancelledResult(this._timeout);
                            }

                            // If the output matches to regex, clean up resources and return the founded output as result.
                            double value = double.Parse(ValueReadingExecutor.NumberMatcher.Match(output).Value, CultureInfo.InvariantCulture);
                            this.CleanUp(process);
                            return new ContinuousResult(value, timer.Elapsed);
                        }
                    },
                cancellationToken: cancellationToken);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up all system resources that have been opened when running the process.
        /// </summary>
        /// <param name="process">The (exited) process.</param>
        private void CleanUp(Process process)
        {
            // Make sure to release system resource received on output redirect(important on Linux!).
            process.StandardOutput.Close();
        }

        #endregion
    }
}