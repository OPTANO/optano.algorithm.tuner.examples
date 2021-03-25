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

namespace Optano.Algorithm.Tuner.Lingeling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Runs the SAT solver Lingeling.
    /// </summary>
    public class LingelingRunner : ITargetAlgorithm<InstanceSeedFile, RuntimeResult>
    {
        #region Fields

        /// <summary>
        /// Algorithm parameters as they should be expressed for the command line.
        /// </summary>
        private readonly string _commandParameters;

        /// <summary>
        /// Path to Lingeling executable.
        /// </summary>
        private readonly string _pathToExecutable;

        /// <summary>
        /// The configuration of the algorithm tuner.
        /// </summary>
        private readonly AlgorithmTunerConfiguration _tunerConfig;

        /// <summary>
        /// The memory limit in megabyte.
        /// </summary>
        private readonly int _memoryLimitMegabyte;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LingelingRunner" /> class.
        /// </summary>
        /// <param name="parameters">Parameters for the Lingeling algorithm.</param>
        /// <param name="pathToExecutable">Path to Lingeling executable.</param>
        /// <param name="tunerConfig">The configuration of the algorithm tuner.</param>
        /// <param name="memoryLimitMegabyte">The memory limit in megabyte.</param>
        public LingelingRunner(
            Dictionary<string, IAllele> parameters,
            string pathToExecutable,
            AlgorithmTunerConfiguration tunerConfig,
            int memoryLimitMegabyte)
        {
            // Render the parameters as accepted by the command line.
            // Set culture to invariant to make sure doubles use periods, not commas.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            this._commandParameters = string.Join(" ", parameters.Select(parameter => $"--{parameter.Key}={parameter.Value}"));

            this._pathToExecutable = pathToExecutable;
            this._tunerConfig = tunerConfig;
            this._memoryLimitMegabyte = memoryLimitMegabyte;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a cancellable task that runs Lingeling on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">Token that is regurlarly checked for cancellation.
        /// If cancellation is detected, the task will be stopped.</param>
        /// <returns>A task that has returns the run's runtime on completion.</returns>
        public Task<RuntimeResult> Run(InstanceSeedFile instance, CancellationToken cancellationToken)
        {
            // Define process to start lingeling.
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
                            this.CleanUp(process, processRegistration);
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        // If the process was not cancelled, first check the output for CPU time.
                        var output = process.StandardOutput.ReadToEnd();

                        // Then clean up resources.
                        this.CleanUp(process, processRegistration);

                        return this.ExtractRunStatistics(output);
                    },
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Extracts the run statistics.
        /// </summary>
        /// <param name="consoleOutput">The console output.</param>
        /// <returns>The run statistics.</returns>
        public RuntimeResult ExtractRunStatistics(string consoleOutput)
        {
            var satMatches = new Regex(@"s SATISFIABLE").Match(consoleOutput);
            var runtimeMatches = new Regex(@"c[ \t]+\d+(?:\.\d+)? seconds,").Match(consoleOutput);

            if (!satMatches.Success || !runtimeMatches.Success)
            {
                return ResultBase<RuntimeResult>.CreateCancelledResult(this._tunerConfig.CpuTimeout);
            }

            var extractedRuntime = new Regex(@"\d+(?:\.\d+)?").Match(runtimeMatches.Value);

            if (!extractedRuntime.Success)
            {
                return ResultBase<RuntimeResult>.CreateCancelledResult(this._tunerConfig.CpuTimeout);
            }

            var extractedRuntimeSeconds = double.Parse(extractedRuntime.Value, CultureInfo.InvariantCulture);
            var reportedRuntime = TimeSpan.FromSeconds(extractedRuntimeSeconds);

            if (reportedRuntime <= this._tunerConfig.CpuTimeout)
            {
                return new RuntimeResult(reportedRuntime);
            }

            return ResultBase<RuntimeResult>.CreateCancelledResult(this._tunerConfig.CpuTimeout);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up all system resources that have been opened when running the process.
        /// </summary>
        /// <param name="process">The (exited) process.</param>
        /// <param name="processRegistration">The processes' registration to the cancellation token.</param>
        private void CleanUp(Process process, CancellationTokenRegistration processRegistration)
        {
            processRegistration.Dispose();

            // Make sure to release system resource received on output redirect(important on Linux!).
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
        /// Builds the <see cref="ProcessStartInfo"/> for starting the Lingeling algorithm on the given instance.
        /// </summary>
        /// <param name="instance">The instance to start the algorithm on.</param>
        /// <returns>The built <see cref="ProcessStartInfo"/>.</returns>
        private ProcessStartInfo BuildProcessStartInfo(InstanceSeedFile instance)
        {
            var memoryLimitKilobyte = this._memoryLimitMegabyte * 1024;
            var memoryLimitScriptPath = Directory.GetCurrentDirectory();
            var memoryLimitExecutable = Path.Combine(memoryLimitScriptPath, "lingelingMemoryLimited.sh");
            var processArguments =
                $"\"{this._pathToExecutable} -T {(int)this._tunerConfig.CpuTimeout.TotalSeconds} -f --seed={instance.Seed} --memlim={this._memoryLimitMegabyte} {this._commandParameters} \"{instance.Path}\"\" {memoryLimitKilobyte}";
            ProcessStartInfo processInfo = new ProcessStartInfo(
                                               fileName: memoryLimitExecutable,
                                               arguments: processArguments)
                                               {
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