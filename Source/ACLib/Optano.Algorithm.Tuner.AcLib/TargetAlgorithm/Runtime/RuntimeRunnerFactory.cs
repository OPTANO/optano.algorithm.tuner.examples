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

namespace Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Runtime
{
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// Creates <see cref="RuntimeRunner"/>s.
    /// </summary>
    internal class RuntimeRunnerFactory : RunnerFactoryBase<RuntimeRunner, RuntimeResult>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeRunnerFactory"/> class.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameterSpecification">
        /// A specification defining, among other things, parameter activity conditions.
        /// </param>
        public RuntimeRunnerFactory(Scenario scenario, ParameterConfigurationSpaceSpecification parameterSpecification)
            : base(scenario, parameterSpecification)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a <see cref="RuntimeRunner" /> configured by the specified parameters.
        /// </summary>
        /// <param name="activeParameters">The parameters, already filtered by
        /// <see cref="ParameterConfigurationSpaceSpecification.ExtractActiveParameters"/>.
        /// </param>
        /// <returns>
        /// The created <see cref="RuntimeRunner" />.
        /// </returns>
        protected override RuntimeRunner CreateTargetAlgorithm(Dictionary<string, IAllele> activeParameters)
        {
            return new RuntimeRunner(this.Scenario, activeParameters);
        }

        #endregion
    }
}