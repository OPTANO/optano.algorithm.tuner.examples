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

namespace Optano.Algorithm.Tuner.Bbob
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.MachineLearning;
    using Optano.Algorithm.Tuner.MachineLearning.Prediction;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;
    using Optano.Algorithm.Tuner.TargetAlgorithm.RunEvaluators;
    using Optano.Algorithm.Tuner.Tuning;

    /// <summary>
    /// Generic entry point for different Saps runner parametrizations.
    /// </summary>
    /// <typeparam name="TLearnerModel">The learner model.</typeparam>
    /// <typeparam name="TPredictorModel">The predictor model.</typeparam>
    /// <typeparam name="TSamplingStrategy">The sampling strategy.</typeparam>
    public static class GenericBbobEntryPoint<TLearnerModel, TPredictorModel, TSamplingStrategy>
        where TLearnerModel : IGenomeLearner<TPredictorModel, TSamplingStrategy>
        where TPredictorModel : IEnsemblePredictor<GenomePredictionTree>
        where TSamplingStrategy : IEnsembleSamplingStrategy<IGenomePredictor>, new()
    {
        #region Public Methods and Operators

        /// <summary>
        /// Starts a generic version of OPTANO Algorithm Tuner, using the specified arguments.
        /// </summary>
        /// <param name="additionalArguments">The additional arguments.</param>
        /// <param name="bbobConfig">The bbob configuration.</param>
        public static void Run(IEnumerable<string> additionalArguments, BbobRunnerConfiguration bbobConfig)
        {
            string pathToInstanceFolder = null;

            var bestParameters =
                Master<BbobRunner,
                        InstanceFile,
                        ContinuousResult,
                        TLearnerModel,
                        TPredictorModel,
                        TSamplingStrategy>
                    .Run(
                        args: additionalArguments.ToArray(),
                        algorithmTunerBuilder:
                        (
                            config,
                            trainingInstances,
                            testInstance) =>
                            {
                                pathToInstanceFolder = trainingInstances ?? "DummyInstances";
                                return GenericBbobEntryPoint<TLearnerModel, TPredictorModel, TSamplingStrategy>.BuildBbobRunner(
                                    config,
                                    pathToInstanceFolder, 
                                    testInstance, 
                                    bbobConfig);
                            });
            // log results
            GenericBbobEntryPoint<TLearnerModel, TPredictorModel, TSamplingStrategy>.LogBestParameters(
                bestParameters,
                bbobConfig,
                pathToInstanceFolder);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs the best parameters.
        /// </summary>
        /// <param name="bestParameters">The best parameters.</param>
        /// <param name="bbobConfig">The bbob configuration.</param>
        /// <param name="pathToInstanceFolder">The path to instance folder.</param>
        private static void LogBestParameters(
            Dictionary<string, IAllele> bestParameters,
            BbobRunnerConfiguration bbobConfig,
            string pathToInstanceFolder)
        {
            var bestParamsConsole = string.Join(
                " ",
                bestParameters?.OrderBy(p => p.Key).Select(p => string.Format(CultureInfo.InvariantCulture, "{0}", (double)p.Value.GetValue())) ?? new string[0]);

            // evaluate the best found config on all instances. print commands to execute python + compute average performance
            var pythonCommand = string.Concat(
                bbobConfig.PythonBin,
                " ",
                bbobConfig.PathToExecutable,
                $" {bbobConfig.FunctionId} {"{0}"} ",
                bestParamsConsole);
            var instances = BbobUtils.CreateInstanceList(pathToInstanceFolder);
            Thread.Sleep(500);
            LoggingHelper.WriteLine(VerbosityLevel.Info, "################################################");
            LoggingHelper.WriteLine(VerbosityLevel.Info, "Commands to evaluate parameters:");

            var instanceResults = new List<double>(instances.Count);
            foreach (var instance in instances)
            {
                var bbobRunner = new BbobRunner(bbobConfig.FunctionId, bestParameters, bbobConfig.PythonBin, bbobConfig.PathToExecutable);
                var runTask = bbobRunner.Run(instance, new CancellationToken(false));

                var currentResult = runTask.Result;

                // read instance id
                var instanceId = -1;
                using (var reader = File.OpenText(instance.Path))
                {
                    instanceId = int.Parse(reader.ReadLine());
                }

                var finalCommand = string.Format(pythonCommand, instanceId);
                LoggingHelper.WriteLine(VerbosityLevel.Info, finalCommand, false);
                LoggingHelper.WriteLine(VerbosityLevel.Info, FormattableString.Invariant($"result={currentResult.Value}"), false);
                instanceResults.Add(currentResult.Value);
            }

            LoggingHelper.WriteLine(VerbosityLevel.Info, "################################################");
            var averageResult = instanceResults.Any() ? instanceResults.Average() : double.NaN;
            if (!File.Exists("averageResults.csv"))
            {
                var header = string.Join(";", Enumerable.Range(0, instances.Count).Select(i => $"instance_{i}")) + ";average\r\n";
                File.WriteAllText("averageResults.csv", header);
            }

            var averageInvariant = string.Format(CultureInfo.InvariantCulture, "{0}\r\n", averageResult);
            var resultLine = string.Join(";", instanceResults.Select(r => string.Format(CultureInfo.InvariantCulture, "{0}", r)));
            File.AppendAllText("averageResults.csv", string.Concat(resultLine, ";", averageInvariant));
            LoggingHelper.WriteLine(VerbosityLevel.Info, FormattableString.Invariant($"Average Result={averageResult}"));
        }

        /// <summary>
        /// Builds an instance of the <see cref="AlgorithmTuner{TTargetAlgorihtm,TInsance,TResult}" /> class which
        /// executes a target function minimization for the BBOB algorithm.
        /// </summary>
        /// <param name="configuration">The <see cref="AlgorithmTunerConfiguration" /> to use.</param>
        /// <param name="trainingInstanceFolder">The training instance folder.</param>
        /// <param name="testInstanceFolder">The test instance folder.</param>
        /// <param name="bbobRunnerConfig">The bbob runner configuration.</param>
        /// <returns>
        /// The built instance.
        /// </returns>
        private static AlgorithmTuner<BbobRunner, InstanceFile, ContinuousResult, TLearnerModel, TPredictorModel, TSamplingStrategy> BuildBbobRunner(
            AlgorithmTunerConfiguration configuration,
            string trainingInstanceFolder,
            string testInstanceFolder,
            BbobRunnerConfiguration bbobRunnerConfig)
        {
            var requiredInstances = (int)Math.Max(configuration.StartNumInstances, configuration.EndNumInstances);
            var random = new Random(bbobRunnerConfig.InstanceSeed);

            var tuner = new AlgorithmTuner<BbobRunner, InstanceFile, ContinuousResult, TLearnerModel, TPredictorModel, TSamplingStrategy>(
                targetAlgorithmFactory: new BbobRunnerFactory(
                    bbobRunnerConfig.PythonBin,
                    bbobRunnerConfig.PathToExecutable,
                    bbobRunnerConfig.FunctionId),
                runEvaluator: new SortByValue(@ascending: true),
                trainingInstances: BbobUtils.CreateInstancesFilesAndReturnAsList(trainingInstanceFolder, requiredInstances, random),
                parameterTree: BbobUtils.CreateParameterTree(bbobRunnerConfig.Dimensions),
                configuration: configuration);

            if (!string.IsNullOrWhiteSpace(testInstanceFolder))
            {
                tuner.SetTestInstances(BbobUtils.CreateInstancesFilesAndReturnAsList(testInstanceFolder, requiredInstances, random));
            }

            return tuner;
        }

        #endregion
    }
}
