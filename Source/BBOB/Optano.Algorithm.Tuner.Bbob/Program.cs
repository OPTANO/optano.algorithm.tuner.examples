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
    using System.Globalization;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestOutOfBox;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestTopPerformerFocus;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData.SamplingStrategies;

    /// <summary>
    /// Program to tune BBOB via OPTANO Algorithm Tuner.
    /// </summary>
    public class Program
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
            LoggingHelper.Configure($"parserLog_{ProcessUtils.GetCurrentProcessId()}.log");

            // Parse arguments.
            var argsParser = new BbobRunnerConfigurationParser();
            if (!ArgumentParserUtils.ParseArguments(argsParser, args))
            {
                return;
            }

            var bbobConfig = argsParser.ConfigurationBuilder.Build();

            if (argsParser.IsMaster)
            {
                switch (bbobConfig.GenericParameterization)
                {
                    case GenericParameterization.RandomForestAverageRank:
                        GenericBbobEntryPoint<
                            GenomePredictionRandomForest<AverageRankStrategy>,
                            GenomePredictionForestModel<GenomePredictionTree>,
                            AverageRankStrategy>.Run(argsParser.AdditionalArguments, bbobConfig);
                        break;
                    case GenericParameterization.RandomForestReuseOldTrees:
                        GenericBbobEntryPoint<
                            GenomePredictionRandomForest<ReuseOldTreesStrategy>,
                            GenomePredictionForestModel<GenomePredictionTree>,
                            ReuseOldTreesStrategy>.Run(argsParser.AdditionalArguments, bbobConfig);
                        break;
                    case GenericParameterization.StandardRandomForest:
                    case GenericParameterization.Default:
                    default:
                        GenericBbobEntryPoint<
                            StandardRandomForestLearner<ReuseOldTreesStrategy>,
                            GenomePredictionForestModel<GenomePredictionTree>,
                            ReuseOldTreesStrategy>.Run(argsParser.AdditionalArguments, bbobConfig);
                        break;
                }
            }

            else
            {
                Worker.Run(args);
                return;
            }
        }

        #endregion
    }
}