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
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A simple implementation of <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/>.
    /// </summary>
    public class LingelingRunnerFactory : ITargetAlgorithmFactory<LingelingRunner, InstanceSeedFile, RuntimeResult>
    {
        #region Fields

        /// <summary>
        /// Path to Lingeling executable.
        /// </summary>
        private readonly string _pathToExecutable;

        /// <summary>
        /// The configuration of the algorithm tuner.
        /// </summary>
        private readonly AlgorithmTunerConfiguration _tunerConfig;

        /// <summary>
        /// The memory limit in megabyte.
        /// </summary>
        private readonly int _memoryLimitMegabyte;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LingelingRunnerFactory" /> class.
        /// </summary>
        /// <param name="pathToExecutable">Path to Lingeling executable.</param>
        /// <param name="tunerConfig">The tunerConfig of the algorithm tuner.</param>
        /// <param name="memoryLimitMegabyte">The memory limit in megabyte.</param>
        public LingelingRunnerFactory(string pathToExecutable, AlgorithmTunerConfiguration tunerConfig, int memoryLimitMegabyte)
        {
            this._pathToExecutable = pathToExecutable;
            this._tunerConfig = tunerConfig;
            this._memoryLimitMegabyte = memoryLimitMegabyte;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Configures a Lingeling instance using the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to configure the Lingeling instance with.</param>
        /// <returns>
        /// The configured Lingeling instance.
        /// </returns>
        public LingelingRunner ConfigureTargetAlgorithm(Dictionary<string, IAllele> parameters)
        {
            return new LingelingRunner(parameters, this._pathToExecutable, this._tunerConfig, this._memoryLimitMegabyte);
        }

        #endregion
    }
}