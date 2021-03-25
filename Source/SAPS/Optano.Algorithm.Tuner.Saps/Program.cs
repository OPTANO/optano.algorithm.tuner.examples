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
    using System.Globalization;
    using System.Linq;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestOutOfBox;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestTopPerformerFocus;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData.SamplingStrategies;

    /// <summary>
    /// Program to tune SAPS via OPTANO Algorithm Tuner.
    /// </summary>
    public static class Program
    {
        #region Public Methods and Operators

        /// <summary>
        /// Entry point to the program.
        /// </summary>
        /// <param name="args">If 'master' is included as argument, a
        /// <see cref="Master{TTargetAlgorithm,TInstance,TResult}"/> is starting using the remaining arguments.
        /// Otherwise, a <see cref="Worker"/> is started with the provided arguments.</param>
        public static void Main(string[] args)
        {
            ProcessUtils.SetDefaultCultureInfo(CultureInfo.InvariantCulture);

            LoggingHelper.Configure($"sapsParserLog.txt");

            // Parse arguments.
            var argsParser = new SapsRunnerConfigurationParser();

            if (!ArgumentParserUtils.ParseArguments(argsParser, args))
            {
                return;
            }

            var config = argsParser.ConfigurationBuilder.Build();

            // Start master or worker depending on arguments.
            if (config.IsMaster)
            {
                Dictionary<string, IAllele> bestParameters;
                switch (config.GenericParameterization)
                {
                    case GenericParameterization.RandomForestAverageRank:
                        bestParameters = GenericSapsEntryPoint<
                            GenomePredictionRandomForest<AverageRankStrategy>,
                            GenomePredictionForestModel<GenomePredictionTree>,
                            AverageRankStrategy>.Run(argsParser.RemainingArguments, config);
                        break;
                    case GenericParameterization.RandomForestReuseOldTrees:
                        bestParameters = GenericSapsEntryPoint<
                            GenomePredictionRandomForest<ReuseOldTreesStrategy>,
                            GenomePredictionForestModel<GenomePredictionTree>,
                            ReuseOldTreesStrategy>.Run(argsParser.RemainingArguments, config);
                        break;
                    case GenericParameterization.StandardRandomForest:
                    case GenericParameterization.Default:
                    default:
                        bestParameters = GenericSapsEntryPoint<
                            StandardRandomForestLearner<ReuseOldTreesStrategy>,
                            GenomePredictionForestModel<GenomePredictionTree>,
                            ReuseOldTreesStrategy>.Run(argsParser.RemainingArguments, config);
                        break;
                }

                Program.LogBestParameters(bestParameters, config);
            }
            else
            {
                Worker.Run(args);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs the best parameters.
        /// </summary>
        /// <param name="bestParameters">The parameters.</param>
        /// <param name="sapsConfig">The config.</param>
        private static void LogBestParameters(Dictionary<string, IAllele> bestParameters, SapsRunnerConfiguration sapsConfig)
        {
            var bestParametersConsoleFormat = string.Join(" ", bestParameters.Select(parameter => $"-{parameter.Key} {parameter.Value}"));
            var ubcsatCommand =
                $"{sapsConfig.PathToExecutable} -alg saps -i [INSTANCE] -timeout [TIMEOUT] -seed [SEED] -cutoff max {bestParametersConsoleFormat}";
            LoggingHelper.WriteLine(VerbosityLevel.Info, "Command to start ubcsat with best parameters:");
            LoggingHelper.WriteLine(VerbosityLevel.Info, ubcsatCommand);
        }

        #endregion
    }
}