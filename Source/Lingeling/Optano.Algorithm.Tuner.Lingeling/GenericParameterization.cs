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

namespace Optano.Algorithm.Tuner.Lingeling
{
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestOutOfBox;
    using Optano.Algorithm.Tuner.MachineLearning.RandomForest.RandomForestTopPerformerFocus;
    using Optano.Algorithm.Tuner.MachineLearning.TrainingData.SamplingStrategies;

    /// <summary>
    /// The available generic parameterizations.
    /// </summary>
    public enum GenericParameterization
    {
        /// <summary>
        /// Default resolves to <see cref="RandomForestReuseOldTrees"/>.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Use
        /// <c>TLearnerModel</c> : <see cref="GenomePredictionRandomForest{TSamplingStrategy}"/>
        /// <c>TPredictorModel</c> : <see cref="GenomePredictionForestModel{TWeakPredictor}"/> where <c>TWeakPredicor</c> : <see cref="GenomePredictionTree"/>
        /// <c>TSamplingStrategy</c> : <see cref="ReuseOldTreesStrategy"/>
        /// </summary>
        RandomForestReuseOldTrees = 1,

        /// <summary>
        /// Use
        /// <c>TLearnerModel</c> : <see cref="GenomePredictionRandomForest{TSamplingStrategy}"/>
        /// <c>TPredictorModel</c> : <see cref="GenomePredictionForestModel{TWeakPredictor}"/> where <c>TWeakPredicor</c> : <see cref="GenomePredictionTree"/>
        /// <c>TSamplingStrategy</c> : <see cref="AverageRankStrategy"/>
        /// </summary>
        RandomForestAverageRank = 2,

        /// <summary>
        /// Use
        /// <c>TLearnerModel</c> : <see cref="GenomePredictionRandomForest{TSamplingStrategy}"/>
        /// <c>TPredictorModel</c> : <see cref="StandardRandomForestLearner{TSamplingStrategy}"/> where <c>TWeakPredicor</c> : <see cref="GenomePredictionTree"/>
        /// <c>TSamplingStrategy</c> : <see cref="ReuseOldTreesStrategy"/>
        /// </summary>
        StandardRandomForest = 3,
    }
}