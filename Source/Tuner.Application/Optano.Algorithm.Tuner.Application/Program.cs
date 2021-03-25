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

using static Optano.Algorithm.Tuner.Logging.LoggingHelper;

namespace Optano.Algorithm.Tuner.Application
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.ParameterTreeReader;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;
    using Optano.Algorithm.Tuner.TargetAlgorithm.RunEvaluators;
    using Optano.Algorithm.Tuner.Tuning;

    /// <summary>
    /// Program to tune arbitrary commands via OPTANO Algorithm Tuner.
    /// </summary>
    public static class Program
    {
        #region Public Methods and Operators

        /// <summary>
        /// Entry point to the program.
        /// </summary>
        /// <param name="args">Program arguments. Call the program with --help for more information.</param>
        public static void Main(string[] args)
        {
            ProcessUtils.SetDefaultCultureInfo(CultureInfo.InvariantCulture);

            // Parse arguments.
            var argsParser = new ArgumentParser();
            if (!ArgumentParserUtils.ParseArguments(argsParser, args))
            {
                return;
            }

            // Start master or worker depending on arguments.
            if (argsParser.MasterRequested)
            {
                Program.RunMaster(argsParser);
            }
            else
            {
                Worker.Run(argsParser.AdditionalArguments.ToArray());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the OPTANO Algorithm Tuner master.
        /// </summary>
        /// <param name="argsParser">An <see cref="ArgumentParser"/> that was already used to parse the command line
        /// arguments.</param>
        private static void RunMaster(ArgumentParser argsParser)
        {
            // Check if the algorithm should be tuned by value or runtime and start the correct run.
            if (argsParser.TuneByValue)
            {
                Master<ValueReadingExecutor, InstanceFile, ContinuousResult>.Run(
                    args: argsParser.AdditionalArguments.ToArray(),
                    algorithmTunerBuilder: (config, trainingInstanceFolder, testInstanceFolder) =>
                        BuildValueTuner(
                            config,
                            trainingInstanceFolder,
                            testInstanceFolder,
                            argsParser.BasicCommand,
                            argsParser.PathToParameterTree,
                            argsParser.SortValuesAscendingly));
            }
            else
            {
                Master<TimeMeasuringExecutor, InstanceFile, RuntimeResult>.Run(
                    args: argsParser.AdditionalArguments.ToArray(),
                    algorithmTunerBuilder: (config, trainingInstanceFolder, testInstanceFolder) =>
                        BuildRuntimeTuner(
                            config,
                            trainingInstanceFolder,
                            testInstanceFolder,
                            argsParser.BasicCommand,
                            argsParser.PathToParameterTree,
                            argsParser.FactorPar));
            }
        }

        /// <summary>
        /// Builds a OPTANO Algorithm Tuner instance that tunes by last value printed to console.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="trainingInstanceFolder">The path to the folder containing training instances.</param>
        /// <param name="testInstanceFolder">The path to the folder containing test instances.</param>
        /// <param name="basicCommand">The basic command to the target algorithm as it should be executed by the
        /// command line. The path to the instance file and the parameters will be set by replacing
        /// <see cref="CommandExecutorBase{TResult}.InstanceReplacement"/> and
        /// <see cref="CommandExecutorBase{TResult}.ParameterReplacement"/>.</param>
        /// <param name="pathToParameterTree">The path to a parameter tree defined via XML.</param>
        /// <param name="ascending">Whether smaller values are better.</param>
        /// <returns>The built OPTANO Algorithm Tuner instance.</returns>
        private static AlgorithmTuner<ValueReadingExecutor, InstanceFile, ContinuousResult> BuildValueTuner(
            AlgorithmTunerConfiguration config,
            string trainingInstanceFolder,
            string testInstanceFolder,
            string basicCommand,
            string pathToParameterTree,
            bool ascending)
        {
            var tuner = new AlgorithmTuner<ValueReadingExecutor, InstanceFile, ContinuousResult>(
                targetAlgorithmFactory: new ValueReadingExecutorFactory(basicCommand, config.CpuTimeout),
                runEvaluator: new SortByValue<InstanceFile>(ascending),
                trainingInstances: ExtractInstances(trainingInstanceFolder),
                parameterTree: ParameterTreeConverter.ConvertToParameterTree(pathToParameterTree),
                configuration: config);
            if (testInstanceFolder != null)
            {
                tuner.SetTestInstances(ExtractInstances(testInstanceFolder));
            }

            return tuner;
        }

        /// <summary>
        /// Builds a OPTANO Algorithm Tuner instance that tunes by process runtime.
        /// </summary>
        /// <param name="config">
        /// The configuration.
        /// </param>
        /// <param name="trainingInstanceFolder">
        /// The path to the folder containing training instances.
        /// </param>
        /// <param name="testInstanceFolder">
        /// The path to the folder containing test instances.
        /// </param>
        /// <param name="basicCommand">
        /// The basic command to the target algorithm as it should be executed by the
        /// command line. The path to the instance file and the parameters will be set by replacing
        /// <see cref="CommandExecutorBase{TResult}.InstanceReplacement"/> and
        /// <see cref="CommandExecutorBase{TResult}.ParameterReplacement"/>.
        /// </param>
        /// <param name="pathToParameterTree">
        /// The path to a parameter tree defined via XML.
        /// </param>
        /// <param name="factorParK">
        /// The PAR-k factor.
        /// </param>
        /// <returns>
        /// The built OPTANO Algorithm Tuner instance.
        /// </returns>
        private static AlgorithmTuner<TimeMeasuringExecutor, InstanceFile, RuntimeResult> BuildRuntimeTuner(
            AlgorithmTunerConfiguration config,
            string trainingInstanceFolder,
            string testInstanceFolder,
            string basicCommand,
            string pathToParameterTree,
            int factorParK)
        {
            var tuner = new AlgorithmTuner<TimeMeasuringExecutor, InstanceFile, RuntimeResult>(
                targetAlgorithmFactory: new TimeMeasuringExecutorFactory(basicCommand, config.CpuTimeout),
                runEvaluator: new SortByRuntime<InstanceFile>(factorParK),
                trainingInstances: ExtractInstances(trainingInstanceFolder),
                parameterTree: ParameterTreeConverter.ConvertToParameterTree(pathToParameterTree),
                configuration: config);
            if (testInstanceFolder != null)
            {
                tuner.SetTestInstances(ExtractInstances(testInstanceFolder));
            }

            return tuner;
        }

        /// <summary>
        /// Extracts all files from a folder and returns them as <see cref="InstanceFile"/>s.
        /// </summary>
        /// <param name="pathToInstanceFolder">Path to the folder to find instances in.</param>
        /// <returns>The extracted instances.</returns>
        private static List<InstanceFile> ExtractInstances(string pathToInstanceFolder)
        {
            try
            {
                // Find all files in directory and set them as instances.
                DirectoryInfo instanceDirectory = new DirectoryInfo(pathToInstanceFolder);
                return instanceDirectory.EnumerateFiles()
                    .Select(file => new InstanceFile(file.FullName))
                    .ToList();
            }
            catch (Exception e)
            {
                // Echo information and rethrow exception if that was not possible.
                WriteLine(VerbosityLevel.Warn, e.Message);
                WriteLine(VerbosityLevel.Warn, "Cannot open folder.");
                throw;
            }
        }

        #endregion
    }
}