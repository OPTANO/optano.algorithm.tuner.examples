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

namespace Optano.Algorithm.Tuner.AcLib
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace;
    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Quality;
    using Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Runtime;
    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestOutOfBox;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestTopPerformerFocus;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData.SamplingStrategies;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;
    using Optano.Algorithm.Tuner.TargetAlgorithm.RunEvaluators;
    using Optano.Algorithm.Tuner.Tuning;

    /// <summary>
    /// Program to tune ACLib target algorithms via OPTANO Algorithm Tuner.
    /// </summary>
    public class Program
    {
        #region Public Methods and Operators

        /// <summary>
        /// Entry point to the program.
        /// </summary>
        /// <param name="args">Program arguments. Call the program with --help for more information.</param>
        public static void Main(string[] args)
        {
            ProcessUtils.SetDefaultCultureInfo(CultureInfo.InvariantCulture);
            LoggingHelper.Configure($"parserLog_{ProcessUtils.GetCurrentProcessId()}.log");

            // Parse arguments.
            var argsParser = new ArgumentParser();
            if (!ArgumentParserUtils.ParseArguments(argsParser, args))
            {
                return;
            }

            // Start master or worker depending on arguments.
            if (argsParser.IsMaster)
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
            var applicationConfig = argsParser.ConfigurationBuilder.Build();
            var scenario = new Scenario(applicationConfig.PathToScenarioFile);

            var parameterSpecification = ParameterConfigurationSpaceConverter.Convert(scenario.ParameterFile);
            var parameterTree = AcLibUtils.CreateParameterTree(parameterSpecification);

            var trainingInstances = AcLibUtils.CreateInstances(scenario.InstanceFile);
            var testInstances = scenario.TestInstanceFile != null
                                    ? AcLibUtils.CreateInstances(scenario.TestInstanceFile)
                                    : new List<InstanceSeedFile>(0);

            if (scenario.OptimizeQuality)
            {
                Master<QualityRunner, InstanceSeedFile, ContinuousResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
                    GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy>.Run(
                    args: argsParser.AdditionalArguments.ToArray(),
                    algorithmTunerBuilder: (configuration, pathToInstanceFolder, pathToTestInstanceFolder)
                        => Program.BuildValueTuner(parameterTree, scenario, parameterSpecification, trainingInstances, testInstances, configuration));
            }
            else
            {
                Master<RuntimeRunner, InstanceSeedFile, RuntimeResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
                    GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy>.Run(
                    args: argsParser.AdditionalArguments.ToArray(),
                    algorithmTunerBuilder: (configuration, pathToInstanceFolder, pathToTestInstanceFolder)
                        => Program.BuildRuntimeTuner(
                            parameterTree,
                            scenario,
                            parameterSpecification,
                            trainingInstances,
                            testInstances,
                            configuration));
            }
        }

        /// <summary>
        /// Builds an instance of OPTANO Algorithm Tuner which tunes for value.
        /// </summary>
        /// <param name="parameterTree">Specification of tune-able parameters.</param>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameterSpecification">
        /// A specification defining forbidden parameter combinations and activity conditions.
        /// </param>
        /// <param name="trainingInstances">The instances to use in tuning.</param>
        /// <param name="testInstances">The instances to use in testing.</param>
        /// <param name="configuration">The <see cref="AlgorithmTunerConfiguration"/>.</param>
        /// <returns>The built instance.</returns>
        private static AlgorithmTuner<QualityRunner, InstanceSeedFile, ContinuousResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
            GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy> BuildValueTuner(
            ParameterTree parameterTree,
            Scenario scenario,
            ParameterConfigurationSpaceSpecification parameterSpecification,
            IEnumerable<InstanceSeedFile> trainingInstances,
            IEnumerable<InstanceSeedFile> testInstances,
            AlgorithmTunerConfiguration configuration)
        {
            LoggingHelper.WriteLine(VerbosityLevel.Info, $"Tuning '{scenario.Command}' for minimum cost.");
            var qualityTuner =
                new AlgorithmTuner<QualityRunner, InstanceSeedFile, ContinuousResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
                    GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy>(
                    targetAlgorithmFactory: new QualityRunnerFactory(scenario, parameterSpecification),
                    runEvaluator: new SortByValue<InstanceSeedFile>(ascending: true),
                    trainingInstances: trainingInstances,
                    parameterTree: parameterTree,
                    configuration: configuration,
                    genomeBuilder: new ParameterConfigurationSpaceGenomeBuilder(parameterTree, parameterSpecification, configuration));
            qualityTuner.SetTestInstances(testInstances);
            return qualityTuner;
        }

        /// <summary>
        /// Builds an instance of OPTANO Algorithm Tuner which tunes for runtime.
        /// </summary>
        /// <param name="parameterTree">Specification of tune-able parameters.</param>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameterSpecification">
        /// A specification defining forbidden parameter combinations and activity conditions.
        /// </param>
        /// <param name="trainingInstances">The instances to use in tuning.</param>
        /// <param name="testInstances">The instances to use in testing.</param>
        /// <param name="configuration">The <see cref="AlgorithmTunerConfiguration"/>.</param>
        /// <returns>The built instance.</returns>
        private static AlgorithmTuner<RuntimeRunner, InstanceSeedFile, RuntimeResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
            GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy> BuildRuntimeTuner(
            ParameterTree parameterTree,
            Scenario scenario,
            ParameterConfigurationSpaceSpecification parameterSpecification,
            IEnumerable<InstanceSeedFile> trainingInstances,
            IEnumerable<InstanceSeedFile> testInstances,
            AlgorithmTunerConfiguration configuration)
        {
            LoggingHelper.WriteLine(VerbosityLevel.Info, $"Tuning '{scenario.Command}' for PAR-{scenario.PenalizationFactor}.");
            var runtimeTuner =
                new AlgorithmTuner<RuntimeRunner, InstanceSeedFile, RuntimeResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
                    GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy>(
                    targetAlgorithmFactory: new RuntimeRunnerFactory(scenario, parameterSpecification),
                    runEvaluator: new SortByPenalizedRuntime<InstanceSeedFile>(scenario.PenalizationFactor, configuration.CpuTimeout),
                    trainingInstances: trainingInstances,
                    parameterTree: parameterTree,
                    configuration: configuration,
                    genomeBuilder: new ParameterConfigurationSpaceGenomeBuilder(parameterTree, parameterSpecification, configuration));
            runtimeTuner.SetTestInstances(testInstances);
            return runtimeTuner;
        }

        #endregion
    }
}