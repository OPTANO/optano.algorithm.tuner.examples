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
    using System.Globalization;
    using System.Linq;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.GrayBox.PostTuningRunner;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestOutOfBox;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestTopPerformerFocus;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData.SamplingStrategies;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.Tuning;

    /// <summary>
    /// Program to tune Gurobi via OPTANO Algorithm Tuner.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class Program
    {
        #region Public Methods and Operators

        /// <summary>
        /// Entry point to the program.
        /// </summary>
        /// <param name="args">If 'master' is included as argument, a
        /// <see cref="Master{TTargetAlgorithm,TInstance,TResult}"/> is starting using the provided arguments.
        /// Otherwise, a <see cref="Worker"/> is started with the provided arguments.</param>
        public static void Main(string[] args)
        {
            ProcessUtils.SetDefaultCultureInfo(CultureInfo.InvariantCulture);
            LoggingHelper.Configure($"parserLog_{ProcessUtils.GetCurrentProcessId()}.log");

            // Parse gurobi configuration.
            var gurobiParser = new GurobiRunnerConfigurationParser();
            if (!ArgumentParserUtils.ParseArguments(gurobiParser, args))
            {
                return;
            }

            if (gurobiParser.IsPostTuningRunner)
            {
                LoggingHelper.Configure($"consoleOutput_PostTuningRun_{ProcessUtils.GetCurrentProcessId()}.log");

                // Parse and build tuner configuration.
                var masterArgumentParser = new MasterArgumentParser();
                if (!ArgumentParserUtils.ParseArguments(masterArgumentParser, gurobiParser.AdditionalArguments.ToArray()))
                {
                    return;
                }

                var tunerConfig = masterArgumentParser.ConfigurationBuilder.Build();
                LoggingHelper.ChangeConsoleLoggingLevel(tunerConfig.Verbosity);

                // Build gurobi configuration.
                var gurobiConfig = Program.BuildGurobiConfigAndCheckThreadCount(gurobiParser.ConfigurationBuilder, tunerConfig);
                var gurobiRunnerFactory = new GurobiRunnerFactory(gurobiConfig, tunerConfig);
                var parameterTree = GurobiUtils.CreateParameterTree();

                // Start post tuning runner.
                var parallelPostTuningRunner =
                    new ParallelPostTuningRunner<GurobiRunner, InstanceSeedFile, GurobiResult>(
                        tunerConfig,
                        gurobiParser.PostTuningConfiguration,
                        gurobiRunnerFactory,
                        parameterTree);
                parallelPostTuningRunner.ExecutePostTuningRunsInParallel();

                return;
            }

            if (gurobiParser.IsMaster)
            {
                Master<GurobiRunner, InstanceSeedFile, GurobiResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
                    GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy>.Run(
                    args: gurobiParser.AdditionalArguments.ToArray(),
                    algorithmTunerBuilder: (algorithmTunerConfig, pathToInstanceFolder, pathToTestInstanceFolder) =>
                        Program.BuildGurobiRunner(
                            algorithmTunerConfig,
                            pathToInstanceFolder,
                            pathToTestInstanceFolder,
                            gurobiParser.ConfigurationBuilder));
            }
            else
            {
                Worker.Run(gurobiParser.AdditionalArguments.ToArray());
            }
        }

        /// <summary>
        /// Builds an instance of the <see cref="AlgorithmTuner{TTargetAlorithm,TInstance,TResult}" /> class for tuning Gurobi.
        /// </summary>
        /// <param name="tunerConfig">The <see cref="AlgorithmTunerConfiguration" /> to use.</param>
        /// <param name="pathToTrainingInstanceFolder">The path to the folder containing training instances.</param>
        /// <param name="pathToTestInstanceFolder">The path to test instance folder.</param>
        /// <param name="gurobiConfigBuilder">The gurobi configuration builder.</param>
        /// <returns>
        /// The built instance.
        /// </returns>
        public static AlgorithmTuner<GurobiRunner, InstanceSeedFile, GurobiResult> BuildGurobiRunner(
            AlgorithmTunerConfiguration tunerConfig,
            string pathToTrainingInstanceFolder,
            string pathToTestInstanceFolder,
            GurobiRunnerConfiguration.GurobiRunnerConfigBuilder gurobiConfigBuilder)
        {
            var gurobiConfig = Program.BuildGurobiConfigAndCheckThreadCount(gurobiConfigBuilder, tunerConfig);

            var tuner = new AlgorithmTuner<GurobiRunner, InstanceSeedFile, GurobiResult>(
                targetAlgorithmFactory: new GurobiRunnerFactory(gurobiConfig, tunerConfig),
                runEvaluator: new GurobiRunEvaluator(tunerConfig.CpuTimeout, gurobiConfig.TertiaryTuneCriterion),
                trainingInstances: InstanceSeedFile.CreateInstanceSeedFilesFromDirectory(
                    pathToTrainingInstanceFolder,
                    GurobiUtils.ListOfValidFileExtensions,
                    gurobiConfig.NumberOfSeeds,
                    gurobiConfig.RngSeed),
                parameterTree: GurobiUtils.CreateParameterTree(),
                configuration: tunerConfig,
                customGrayBoxMethods: new GurobiGrayBoxMethods());

            try
            {
                if (!string.IsNullOrWhiteSpace(pathToTestInstanceFolder))
                {
                    var testInstances = InstanceSeedFile.CreateInstanceSeedFilesFromDirectory(
                        pathToTestInstanceFolder,
                        GurobiUtils.ListOfValidFileExtensions,
                        gurobiConfig.NumberOfSeeds,
                        gurobiConfig.RngSeed);
                    tuner.SetTestInstances(testInstances);
                }
            }
            catch
            {
            }

            return tuner;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the <see cref="GurobiRunnerConfiguration"/> and checks the thread count.
        /// </summary>
        /// <param name="gurobiConfigBuilder">The <see cref="GurobiRunnerConfiguration.GurobiRunnerConfigBuilder"/>.</param>
        /// <param name="tunerConfig">The <see cref="AlgorithmTunerConfiguration"/>.</param>
        /// <returns>The <see cref="GurobiRunnerConfiguration"/>.</returns>
        private static GurobiRunnerConfiguration BuildGurobiConfigAndCheckThreadCount(
            GurobiRunnerConfiguration.GurobiRunnerConfigBuilder gurobiConfigBuilder,
            AlgorithmTunerConfiguration tunerConfig)
        {
            var gurobiConfig = gurobiConfigBuilder.Build(tunerConfig.CpuTimeout);

            if (gurobiConfig.ThreadCount * tunerConfig.MaximumNumberParallelEvaluations > Environment.ProcessorCount)
            {
                LoggingHelper.WriteLine(
                    VerbosityLevel.Warn,
                    $"Warning: You specified {tunerConfig.MaximumNumberParallelEvaluations} parallel evaluations with {gurobiConfig.ThreadCount} threads each, but only have {Environment.ProcessorCount} processors. Processes may fight for resources.");
            }

            return gurobiConfig;
        }

        #endregion
    }
}