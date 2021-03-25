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
    using System.Collections.Generic;
    using System.Linq;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Genomes.Values;
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
    public static class GenericSapsEntryPoint<TLearnerModel, TPredictorModel, TSamplingStrategy>
        where TLearnerModel : IGenomeLearner<TPredictorModel, TSamplingStrategy>
        where TPredictorModel : IEnsemblePredictor<GenomePredictionTree>
        where TSamplingStrategy : IEnsembleSamplingStrategy<IGenomePredictor>, new()
    {
        #region Public Methods and Operators

        /// <summary>
        /// Runs the master.
        /// </summary>
        /// <param name="remainingArguments">Remaining arguments for the tuner.</param>
        /// <param name="runnerConfig">Configuration of the algorithm.</param>
        /// <returns>The best parameters.</returns>
        public static Dictionary<string, IAllele> Run(IEnumerable<string> remainingArguments, SapsRunnerConfiguration runnerConfig)
        {
            var bestParameters = Master<SapsRunner, InstanceSeedFile, RuntimeResult, TLearnerModel, TPredictorModel, TSamplingStrategy>.Run(
                args: remainingArguments.ToArray(),
                algorithmTunerBuilder: (tunerConfig, trainingInstanceFolder, testInstanceFolder) =>
                    GenericSapsEntryPoint<TLearnerModel, TPredictorModel, TSamplingStrategy>.BuildSapsAlgorithmTuner(
                        tunerConfig,
                        trainingInstanceFolder,
                        testInstanceFolder,
                        runnerConfig));
            return bestParameters;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds an instance of the <see cref="AlgorithmTuner{SapsRunner, SatInstance, RuntimeResult}"/> class which
        /// executes a runtime minimization for the SAPS algorithm.
        /// </summary>
        /// <param name="tunerConfig">The <see cref="AlgorithmTunerConfiguration"/> to use.</param>
        /// <param name="trainingInstanceFolder">The path to the folder containing training instances.</param>
        /// <param name="testInstanceFolder">The path to the folder containing test instances (optional).</param>
        /// <param name="runnerConfig">The configuration of the Saps runner.</param>
        /// <returns>The built instance.</returns>
        private static AlgorithmTuner<SapsRunner, InstanceSeedFile, RuntimeResult, TLearnerModel, TPredictorModel, TSamplingStrategy>
            BuildSapsAlgorithmTuner(
                AlgorithmTunerConfiguration tunerConfig,
                string trainingInstanceFolder,
                string testInstanceFolder,
                SapsRunnerConfiguration runnerConfig)
        {
            var tuner = new AlgorithmTuner<SapsRunner, InstanceSeedFile, RuntimeResult, TLearnerModel, TPredictorModel, TSamplingStrategy>(
                targetAlgorithmFactory: new SapsRunnerFactory(runnerConfig.PathToExecutable, tunerConfig.CpuTimeout),
                runEvaluator: new SortByRuntime<InstanceSeedFile>(runnerConfig.FactorParK),
                trainingInstances: InstanceSeedFile.CreateInstanceSeedFilesFromDirectory(
                    trainingInstanceFolder,
                    SapsUtils.ListOfValidFileExtensions,
                    runnerConfig.NumberOfSeeds,
                    runnerConfig.RngSeed),
                parameterTree: SapsUtils.CreateParameterTree(),
                configuration: tunerConfig);
            if (!string.IsNullOrWhiteSpace(testInstanceFolder))
            {
                tuner.SetTestInstances(
                    InstanceSeedFile.CreateInstanceSeedFilesFromDirectory(
                        testInstanceFolder,
                        SapsUtils.ListOfValidFileExtensions,
                        runnerConfig.NumberOfSeeds,
                        runnerConfig.RngSeed));
            }

            return tuner;
        }

        #endregion
    }
}