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

namespace Optano.Algorithm.Tuner.AcLib.TargetAlgorithm
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.Result;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// An abstract base class for executing target algorithms meeting ACLib contracts, i. e. algorithms
    /// invokable by "command instance_name instance_specific_information cutoff_time cutoff_length
    /// seed -parameter_id_1 value_1 -parameter_id_2 value_2 ... -parameter_id_n value_n" and printing a single line
    /// of format "Result for ParamILS: status, runtime, runlength, quality, seed, additional data".
    /// </summary>
    /// <typeparam name="TResult">The type of the target algorithm run result wrapper.</typeparam>
    public abstract class RunnerBase<TResult> : ITargetAlgorithm<InstanceSeedFile, TResult>
        where TResult : ResultBase<TResult>, new()
    {
        #region Fields

        /// <summary>
        /// The format in which the wrapper writes the final result. First captured group is status, second the
        /// runtime, third one the quality.
        /// </summary>
        private readonly Regex _resultFormat = new Regex(@"Result for ParamILS: ([^,]*), ([^,]*), (?:[^,]*), ([^,]*), (?:[^\n\r]*)");

        /// <summary>
        /// Additional parameters for the target algorithm in  [-identifier value]* format.
        /// </summary>
        private readonly string _commandParameters;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerBase{TResult}"/> class.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameters">Parameters for the target algorithm.</param>
        protected RunnerBase(Scenario scenario, Dictionary<string, IAllele> parameters)
        {
            this.Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));

            // Render the parameters as accepted by the command line.
            // Set culture to invariant to make sure doubles use periods, not commas.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            this._commandParameters = string.Join(
                " ",
                parameters.Select(parameter => $"-{parameter.Key} {parameter.Value}"));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ACLib scenario.
        /// </summary>
        protected Scenario Scenario { get; }

        /// <summary>
        /// Gets the output read from console.
        /// </summary>
        protected StringBuilder Output { get; } = new StringBuilder();

        /// <summary>
        /// Gets the error read from console.
        /// </summary>
        protected StringBuilder Error { get; } = new StringBuilder();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a cancellable task that runs a target algorithm on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">Token that is regularly checked for cancellation.
        /// If cancellation is detected, the task will be stopped.</param>
        /// <returns>A task that returns statistics about the run on completion.</returns>
        public Task<TResult> Run(InstanceSeedFile instance, CancellationToken cancellationToken)
        {
            // Define process to target algorithm from command line.
            var processInfo = this.BuildProcessStartInfo(instance);

            // Make sure output and Error stores are clean.
            this.Output.Clear();
            this.Error.Clear();

            return Task.Run(
                function: () =>
                    {
                        // Start process and make sure it's cancelled if the cancellationToken is cancelled.
                        using (var process = Process.Start(processInfo))
                        using (var processRegistration =
                            cancellationToken.Register(() => ProcessUtils.CancelProcess(process)))
                        {
                            LoggingHelper.WriteLine(VerbosityLevel.Trace, "Started process.");

                            // Catch all output.
                            // Read streams asynchronously to prevent deadlocks.
                            process.OutputDataReceived += (s, e) => this.Output.AppendLine(e.Data);
                            process.ErrorDataReceived += (s, e) => this.Error.AppendLine(e.Data);
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            // Wait until end of process.
                            process.WaitForExit();

                            if (cancellationToken.IsCancellationRequested)
                            {
                                this.HandleCancellation(process, cancellationToken);
                            }

                            // Parse result.
                            OriginalRunResult originalRunResult;
                            try
                            {
                                originalRunResult = this.ExtractRunResult();
                            }
                            catch (Exception)
                            {
                                // Write information to file if anything goes wrong.
                                this.WriteProcessOutputToFiles(process);
                                throw;
                            }

                            var result = this.CreateRunResult(originalRunResult);
                            LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Result: {result}");

                            return result;
                        }
                    },
                cancellationToken: cancellationToken);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles a cancellation provoked by a cancellation token.
        /// </summary>
        /// <param name="process">The process that was cancelled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected abstract void HandleCancellation(Process process, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="originalRunResult">Results of the target algorithm run as reported by its output.</param>
        /// <returns>The created result.</returns>
        protected abstract TResult CreateRunResult(OriginalRunResult originalRunResult);

        /// <summary>
        /// Writes the process console output collected in <see cref="Output"/> and <see cref="Error"/> to files depending on type.
        /// </summary>
        /// <param name="process">The process. Used to name files.</param>
        protected void WriteProcessOutputToFiles(Process process)

        {
            Directory.CreateDirectory("output");

            var runIdentification = $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}_{process.Id:D5}";

            if (this.Output.Length != 0)
            {
                using (var writer = new StreamWriter($"output/output_console_{runIdentification}.txt", false))
                {
                    writer.Write(this.Output);
                }
            }

            if (this.Error.Length != 0)
            {
                using (var writer = new StreamWriter($"output/output_error_{runIdentification}.txt", false))
                {
                    writer.Write(this.Error);
                }
            }
        }

        /// <summary>
        /// Builds the <see cref="ProcessStartInfo"/> for starting the algorithm on the given instance.
        /// </summary>
        /// <param name="instance">The instance to start the algorithm on.</param>
        /// <returns>The built <see cref="ProcessStartInfo"/>.</returns>
        private ProcessStartInfo BuildProcessStartInfo(InstanceSeedFile instance)
        {
            // Create the process information using the correct program and parameters.
            var command = this.CreateTargetAlgorithmCommand(instance);
            var processInfo = new ProcessStartInfo(
                                  fileName: command.Split(' ').First(),
                                  arguments: command.Substring(command.IndexOf(' ') + 1))
                                  {
                                      // Make sure no additional window will be opened on process start.
                                      CreateNoWindow = true,
                                      UseShellExecute = false,

                                      // Redirect output to read information from it.
                                      RedirectStandardOutput = true,
                                      RedirectStandardError = true,
                                  };

            return processInfo;
        }

        /// <summary>
        /// Creates the command to execute the target algorithm on a specific instance.
        /// </summary>
        /// <param name="instance">The instance to execute the algorithm on.</param>
        /// <returns>The created command.</returns>
        private string CreateTargetAlgorithmCommand(InstanceSeedFile instance)
        {
            // Run length and instance specific information are not used by any relevant target algorithm.
            var instanceSpecificInfoDummy = 0;
            var runLengthDummy = 0;

            // AcLib assumes positive seeds. Map to complete unsigned range by unchecked environment.
            uint positiveSeed;
            unchecked
            {
                positiveSeed = (uint)instance.Seed;
            }

            return
                $"{this.Scenario.Command} {instance.Path} {instanceSpecificInfoDummy} {this.Scenario.CutoffTime.TotalSeconds} {runLengthDummy} {positiveSeed} {this._commandParameters}";
        }

        /// <summary>
        /// Extracts the run result from output.
        /// </summary>
        /// <returns>The created <see cref="OriginalRunResult"/>.</returns>
        private OriginalRunResult ExtractRunResult()
        {
            var result = this._resultFormat.Match(this.Output.ToString());
            if (!result.Success)
            {
                throw new FormatException($"There is no string matching '{this._resultFormat}' in the algorithm output.");
            }

            LoggingHelper.WriteLine(VerbosityLevel.Trace, $"Finished. Relevant output: {result.Value}");

            if (!Enum.TryParse(result.Groups[1].Value, ignoreCase: true, result: out RunStatus status))
            {
                throw new ArgumentException(
                    $"{result.Groups[1].Value} is not a valid status. Valid: {string.Join(", ", Enum.GetNames(typeof(RunStatus)))}.");
            }

            if (status == RunStatus.Abort)
            {
                throw new AggregateException($"Run finished with a status of {status}.");
            }

            if (!double.TryParse(result.Groups[2].Value, out var runtime))
            {
                throw new FormatException($"{result.Groups[2].Value} is not a valid runtime.");
            }

            if (!double.TryParse(result.Groups[3].Value, out var quality))
            {
                throw new FormatException($"{result.Groups[3].Value} is not a valid quality.");
            }

            return new OriginalRunResult(status, TimeSpan.FromSeconds(runtime), quality);
        }

        #endregion
    }
}