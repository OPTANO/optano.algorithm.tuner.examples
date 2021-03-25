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

namespace Optano.Algorithm.Tuner.Application
{
    using System;
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A simple implementation of 
    /// <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/>.
    /// </summary>
    internal class ValueReadingExecutorFactory
        : ITargetAlgorithmFactory<ValueReadingExecutor, InstanceFile, ContinuousResult>
    {
        #region Fields

        /// <summary>
        /// The basic command to the target algorithm as it should be executed by the command line. The path to the
        /// instance file and the parameters will be set by replacing
        /// <see cref="CommandExecutorBase{TResult}.InstanceReplacement"/>
        /// and <see cref="CommandExecutorBase{TResult}.ParameterReplacement"/>.
        /// </summary>
        private readonly string _basicCommand;

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly TimeSpan _timeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueReadingExecutorFactory" /> class.
        /// </summary>
        /// <param name="basicCommand">The basic command to the target algorithm as it should be executed by the
        /// command line. The path to the instance file and the parameters will be set by replacing
        /// <see cref="CommandExecutorBase{TResult}.InstanceReplacement" /> and
        /// <see cref="CommandExecutorBase{TResult}.ParameterReplacement" />.</param>
        /// <param name="timeout">The timeout.</param>
        public ValueReadingExecutorFactory(string basicCommand, TimeSpan timeout)
        {
            this._basicCommand = basicCommand;
            this._timeout = timeout;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Configures a value reading executor using the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to configure the value reading executor with.</param>
        /// <returns>The configured value reading executor.</returns>
        public ValueReadingExecutor ConfigureTargetAlgorithm(Dictionary<string, IAllele> parameters)
        {
            return new ValueReadingExecutor(parameters, this._basicCommand, this._timeout);
        }

        #endregion
    }
}