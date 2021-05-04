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

namespace Optano.Algorithm.Tuner.AcLib.TargetAlgorithm
{
    using System;
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/> for
    /// <see cref="RunnerBase{TResult}"/>s.
    /// </summary>
    /// <typeparam name="TTargetAlgorithm">The type of the target algorithm.</typeparam>
    /// <typeparam name="TResult">The type of the target algorithm result.</typeparam>
    internal abstract class RunnerFactoryBase<TTargetAlgorithm, TResult>
        : ITargetAlgorithmFactory<TTargetAlgorithm, InstanceSeedFile, TResult>
        where TTargetAlgorithm : RunnerBase<TResult>
        where TResult : ResultBase<TResult>, new()
    {
        #region Fields

        /// <summary>
        /// A specification defining, among other things, parameter activity conditions.
        /// </summary>
        private readonly ParameterConfigurationSpaceSpecification _parameterSpecification;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RunnerFactoryBase{TTargetAlgorithm,TResult}"/>
        /// class.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameterSpecification">
        /// A specification defining, among other things, parameter activity conditions.
        /// </param>
        protected RunnerFactoryBase(Scenario scenario, ParameterConfigurationSpaceSpecification parameterSpecification)
        {
            this.Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            this._parameterSpecification = parameterSpecification ??
                                           throw new ArgumentNullException(nameof(parameterSpecification));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ACLib scenario.
        /// </summary>
        protected Scenario Scenario { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Configures a runner instance using the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to configure the runner instance with.</param>
        /// <returns>The configured runner instance.</returns>
        public TTargetAlgorithm ConfigureTargetAlgorithm(Dictionary<string, IAllele> parameters)
        {
            return this.CreateTargetAlgorithm(this._parameterSpecification.ExtractActiveParameters(parameters));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a <typeparamref name="TTargetAlgorithm"/> configured by the specified parameters.
        /// </summary>
        /// <param name="activeParameters">
        /// The parameters, already filtered by
        /// <see cref="ParameterConfigurationSpaceSpecification.ExtractActiveParameters"/>.
        /// </param>
        /// <returns>The created <typeparamref name="TTargetAlgorithm"/>.</returns>
        protected abstract TTargetAlgorithm CreateTargetAlgorithm(Dictionary<string, IAllele> activeParameters);

        #endregion
    }
}