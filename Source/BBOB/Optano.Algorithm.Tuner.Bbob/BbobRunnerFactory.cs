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

namespace Optano.Algorithm.Tuner.Bbob
{
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A simple implementation of <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/>.
    /// </summary>
    public class BbobRunnerFactory : ITargetAlgorithmFactory<BbobRunner, InstanceFile, ContinuousResult>
    {
        #region Fields

        /// <summary>
        /// Path to BBOB executable.
        /// </summary>
        private readonly string _pathToExecutable;

        /// <summary>
        /// The python binary.
        /// </summary>
        private readonly string _pythonBin;

        /// <summary>
        /// The target function id.
        /// </summary>
        private readonly int _targetFunctionId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BbobRunnerFactory" /> class.
        /// </summary>
        /// <param name="pythonBin">The python binary.</param>
        /// <param name="pathToExecutable">Path to BBOB executable.</param>
        /// <param name="targetFunctionId">The target function id.</param>
        public BbobRunnerFactory(string pythonBin, string pathToExecutable, int targetFunctionId)
        {
            this._pythonBin = pythonBin;
            this._pathToExecutable = pathToExecutable;
            this._targetFunctionId = targetFunctionId;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Configures a BBOB instance using the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to configure the BBOB instance with.</param>
        /// <returns>
        /// The configured BBOB instance.
        /// </returns>
        public BbobRunner ConfigureTargetAlgorithm(Dictionary<string, IAllele> parameters)
        {
            return new BbobRunner(this._targetFunctionId, parameters, this._pythonBin, this._pathToExecutable);
        }

        #endregion
    }
}