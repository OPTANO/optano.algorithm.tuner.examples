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

namespace Optano.Algorithm.Tuner.Bbob
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Runs BBOB.
    /// </summary>
    public class BbobRunner : ITargetAlgorithm<InstanceFile, ContinuousResult>
    {
        #region Fields

        /// <summary>
        /// Algorithm parameters as they should be expressed for the command line.
        /// </summary>
        private readonly string _commandParameters;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BbobRunner"/> class.
        /// </summary>
        /// <param name="functionId">The function identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="pythonBin">The python bin.</param>
        /// <param name="pathToScript">The path to script.</param>
        public BbobRunner(int functionId, Dictionary<string, IAllele> parameters, string pythonBin, string pathToScript)
        {
            var pathToBenchmarkScript = Path.Combine(Path.GetDirectoryName(pathToScript), "bbobbenchmarks.py");
            if (!File.Exists(pathToBenchmarkScript))
            {
                throw new FileNotFoundException(
                    "You need to provide bbobbenchmarks.py next to your BBOB python script. For further information please consult the documentation at https://docs.optano.com/algorithmtuner/current/userDoc/bbob.html.");
            }

            // Render the parameters as accepted by the command line.
            // Set culture to invariant to make sure doubles use periods, not commas.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            this._commandParameters = string.Concat(
                $"{pathToScript} {functionId}",
                " {0} ",
                string.Join(
                    " ",
                    parameters.OrderBy(p => p.Key).Select(parameter => $"{parameter.Value}")));

            this.PythonBin = pythonBin;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets path to python's bin folder. Ends with "python".
        /// </summary>
        protected string PythonBin { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Extracts the function value.
        /// </summary>
        /// <param name="consoleOutput">The console output.</param>
        /// <returns>The function value.</returns>
        public static double ExtractFunctionValue(string consoleOutput)
        {
            try
            {
                var resultIndex = consoleOutput.IndexOf("result=", StringComparison.InvariantCulture);
                var resultString = consoleOutput.Substring(resultIndex + "result=".Length).Trim();
                var functionValue = double.Parse(resultString, CultureInfo.InvariantCulture);
                return functionValue;
            }
            catch (Exception)
            {
                return double.MaxValue;
            }
        }

        /// <summary>
        /// Creates a cancellable task that runs the algorithm on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">Token that should regurlarly be checked for cancellation.
        /// If cancellation is detected, the task has to be stopped.</param>
        /// <returns>
        /// A task that returns everything important about the run on completion.
        /// </returns>
        public Task<ContinuousResult> Run(InstanceFile instance, CancellationToken cancellationToken)
        {
            // Define process to start BBOB.
            var processInfo = this.BuildProcessStartInfo(instance);

            return Task.Run(
                function: () =>
                    {
                        // Start process.
                        var timer = new Stopwatch();
                        timer.Start();
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

                        // If the process was not cancelled, first check the console output for result.
                        var output = process.StandardOutput.ReadToEnd();
                        timer.Stop();

                        var functionValue = BbobRunner.ExtractFunctionValue(output);

                        // Then clean up resources.
                        this.CleanUp(process, processRegistration);

                        // Finally return the result.
                        return new ContinuousResult(functionValue, timer.Elapsed);
                    },
                cancellationToken: cancellationToken);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the <see cref="ProcessStartInfo"/> for starting the BBOB algorithm on the given instance.
        /// </summary>
        /// <param name="instance">The instance to start the algorithm on.</param>
        /// <returns>The built <see cref="ProcessStartInfo"/>.</returns>
        private ProcessStartInfo BuildProcessStartInfo(InstanceFile instance)
        {
            // instance file should only contain the instance id in line 0.
            var instanceId = -1;
            using (var reader = File.OpenText(instance.Path))
            {
                instanceId = int.Parse(reader.ReadLine());
            }

            // Create the process information using the correct program and parameters.
            ProcessStartInfo processInfo = new ProcessStartInfo(
                                               fileName: this.PythonBin,
                                               arguments: string.Format(this._commandParameters, instanceId))
                                               {
                                                   CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true,
                                               };

            // Make sure no additional window will be opened on process start.

            // Redirect standard console output to read runtime from console output.

            return processInfo;
        }

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

        #endregion
    }
}