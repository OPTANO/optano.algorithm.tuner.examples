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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestOutOfBox;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestTopPerformerFocus;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData.SamplingStrategies;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.Tuning;

    /// <summary>
    /// Program to tune Gurobi via OPTANO Algorithm Tuner.
    /// </summary>
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
            var parser = new GurobiRunnerConfigurationParser();
            parser.ParseArguments(args);

            if (parser.HelpTextRequested)
            {
                return;
            }

            if (parser.MasterRequested)
            {
                Master<GurobiRunner, InstanceSeedFile, GurobiResult, StandardRandomForestLearner<ReuseOldTreesStrategy>,
                    GenomePredictionForestModel<GenomePredictionTree>, ReuseOldTreesStrategy>.Run(
                    args: parser.RemainingArguments.ToArray(),
                    algorithmTunerBuilder: (algorithmTunerConfig, pathToInstanceFolder, pathToTestInstanceFolder) =>
                        Program.BuildGurobiRunner(algorithmTunerConfig, pathToInstanceFolder, pathToTestInstanceFolder, parser.ConfigurationBuilder));
            }
            else
            {
                Worker.Run(parser.RemainingArguments.ToArray());
            }
        }

        /// <summary>
        /// Builds an instance of the <see cref="AlgorithmTuner{TTargetAlorithm,TInstance,TResult}" /> class for tuning Gurobi.
        /// </summary>
        /// <param name="configuration">The <see cref="AlgorithmTunerConfiguration" /> to use.</param>
        /// <param name="pathToTrainingInstanceFolder">The path to the folder containing training instances.</param>
        /// <param name="pathToTestInstanceFolder">The path to test instance folder.</param>
        /// <param name="gurobiConfigBuilder">The gurobi configuration builder.</param>
        /// <returns>
        /// The built instance.
        /// </returns>
        public static AlgorithmTuner<GurobiRunner, InstanceSeedFile, GurobiResult> BuildGurobiRunner(
            AlgorithmTunerConfiguration configuration,
            string pathToTrainingInstanceFolder,
            string pathToTestInstanceFolder,
            GurobiRunnerConfiguration.GurobiRunnerConfigBuilder gurobiConfigBuilder)
        {
            var gurobiConfig = gurobiConfigBuilder.Build(configuration.CpuTimeout);

            var tuner = new AlgorithmTuner<GurobiRunner, InstanceSeedFile, GurobiResult>(
                targetAlgorithmFactory: new GurobiRunnerFactory(gurobiConfig),
                runEvaluator: new GurobiRunEvaluator(),
                trainingInstances: InstanceSeedFile.CreateInstanceSeedFilesFromDirectory(pathToTrainingInstanceFolder, GurobiUtils.ListOfValidFileExtensions, gurobiConfig.NumberOfSeeds, gurobiConfig.RngSeed),
                parameterTree: GurobiUtils.CreateParameterTree(),
                configuration: configuration);

            try
            {
                if (!string.IsNullOrWhiteSpace(pathToTestInstanceFolder))
                {
                    var testInstances = InstanceSeedFile.CreateInstanceSeedFilesFromDirectory(pathToTestInstanceFolder, GurobiUtils.ListOfValidFileExtensions, gurobiConfig.NumberOfSeeds, gurobiConfig.RngSeed);
                    tuner.SetTestInstances(testInstances);
                }
            }
            catch
            {
            }

            return tuner;
        }

        #endregion
    }
}