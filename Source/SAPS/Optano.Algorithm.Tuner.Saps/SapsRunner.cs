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

namespace Optano.Algorithm.Tuner.Saps
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Runs the SAT solver SAPS.
    /// </summary>
    public class SapsRunner : ITargetAlgorithm<InstanceSeedFile, RuntimeResult>
    {
        #region Static Fields

        /// <summary>
        /// A regular expression that matches the number in the line printing the median CPU time in ubcsat SAPS.
        /// </summary>
        private static readonly Regex CpuTimeMatcher = new Regex(@"CPUTime_Median = ([-+]?\d*\.?\d+([eE][-+]?\d+)?)");

        #endregion

        #region Fields

        /// <summary>
        /// Algorithm parameters as they should be expressed for the command line.
        /// </summary>
        private readonly string _commandParameters;

        /// <summary>
        /// Path to SAPS executable.
        /// </summary>
        private readonly string _pathToExecutable;

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly TimeSpan _timeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SapsRunner" /> class.
        /// </summary>
        /// <param name="parameters">Parameters for the SAPS algorithm.</param>
        /// <param name="pathToExecutable">The path to SAPS executable.</param>
        /// <param name="timeout">The timeout.</param>
        public SapsRunner(Dictionary<string, IAllele> parameters, string pathToExecutable, TimeSpan timeout)
        {
            // Render the parameters as accepted by the command line.
            // Set culture to invariant to make sure doubles use periods, not commas.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            this._commandParameters = string.Join(
                " ",
                parameters.Select(parameter => $"-{parameter.Key} {parameter.Value}"));

            this._pathToExecutable = pathToExecutable;
            this._timeout = timeout;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates the runtime result.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The result.</returns>
        public static RuntimeResult CreateRuntimeResult(string output, TimeSpan timeout)
        {
            // If the process was not cancelled, first check the output for CPU time.
            if (!SapsRunner.CpuTimeMatcher.IsMatch(output))
            {
                return ResultBase<RuntimeResult>.CreateCancelledResult(timeout);
            }

            var printedCpuTime = SapsRunner.CpuTimeMatcher.Match(output);
            var cpuTimeSeconds = double.Parse(printedCpuTime.Groups[1].Value, CultureInfo.InvariantCulture);
            var isTimeout = cpuTimeSeconds + 1e-6 >= timeout.TotalSeconds;

            // Finally return CPU time as result.
            if (!isTimeout)
            {
                return new RuntimeResult(TimeSpan.FromSeconds(cpuTimeSeconds));
            }
            else
            {
                return ResultBase<RuntimeResult>.CreateCancelledResult(timeout);
            }
        }

        /// <summary>
        /// Creates a cancellable task that runs SAPS on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">Token that is regularly checked for cancellation.
        /// If cancellation is detected, the task will be stopped.</param>
        /// <returns>A task that has returns the run's runtime on completion.</returns>
        public Task<RuntimeResult> Run(InstanceSeedFile instance, CancellationToken cancellationToken)
        {
            // Define process to start SAPS.
            var processInfo = this.BuildProcessStartInfo(instance);

            return Task.Run(
                function: () =>
                    {
                        // Start process.
                        using var process = Process.Start(processInfo);

                        // Process needs to be canceled if cancellation token is canceled.
                        var processRegistration = cancellationToken.Register(
                            () => ProcessUtils.CancelProcess(process));

                        // Wait until end of process.
                        process.WaitForExit();

                        // If the process was cancelled, clean up resources and escalate it up.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            SapsRunner.CleanUp(process, processRegistration);
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        // If the process was not cancelled, first check the output for CPU time.
                        var output = process.StandardOutput.ReadToEnd();

                        // Then clean up resources.
                        SapsRunner.CleanUp(process, processRegistration);

                        return SapsRunner.CreateRuntimeResult(output, this._timeout);
                    },
                cancellationToken: cancellationToken);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up all system resources that have been opened when running the process.
        /// </summary>
        /// <param name="process">The (exited) process.</param>
        /// <param name="processRegistration">The process registration.</param>
        private static void CleanUp(Process process, CancellationTokenRegistration processRegistration)
        {
            processRegistration.Dispose();
            // Make sure to release system resource received on output redirect (important on Linux!).
            try
            {
                process.StandardOutput.Close();
                process.Kill();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Builds the <see cref="ProcessStartInfo"/> for starting the SAPS algorithm on the given instance.
        /// </summary>
        /// <param name="instance">The instance to start the algorithm on.</param>
        /// <returns>The built <see cref="ProcessStartInfo"/>.</returns>
        private ProcessStartInfo BuildProcessStartInfo(InstanceSeedFile instance)
        {
            // Create the process information using the correct program and parameters.
            var processInfo = new ProcessStartInfo(
                                  fileName: this._pathToExecutable,
                                  $"-alg saps -i \"{instance.Path}\" {this._commandParameters} -timeout {(int)this._timeout.TotalSeconds} -cutoff max -seed {instance.Seed}")
                                  {
                                      // Make sure no additional window will be opened on process start.
                                      CreateNoWindow = true,
                                      UseShellExecute = false,

                                      // Redirect standard output to read runtime from output.
                                      RedirectStandardOutput = true,
                                  };
            return processInfo;
        }

        #endregion
    }
}