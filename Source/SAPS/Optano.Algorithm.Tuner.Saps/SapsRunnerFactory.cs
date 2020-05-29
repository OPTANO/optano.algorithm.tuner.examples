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

namespace Optano.Algorithm.Tuner.Saps
{
    using System;
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A simple implementation of <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/>.
    /// </summary>
    public class SapsRunnerFactory : ITargetAlgorithmFactory<SapsRunner, InstanceSeedFile, RuntimeResult>
    {
        #region Fields

        /// <summary>
        /// Path to SAPS executable.
        /// </summary>
        private readonly string _pathToExecutable;

        /// <summary>
        /// The timeout.
        /// </summary>
        private readonly TimeSpan _timeout;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SapsRunnerFactory" /> class.
        /// </summary>
        /// <param name="pathToExecutable">Path to SAPS executable.</param>
        /// <param name="timeout">The timeout.</param>
        public SapsRunnerFactory(string pathToExecutable, TimeSpan timeout)
        {
            this._pathToExecutable = pathToExecutable;
            this._timeout = timeout;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Configures a SAPS instance using the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to configure the SAPS instance with.</param>
        /// <returns>The configured SAPS instance.</returns>
        public SapsRunner ConfigureTargetAlgorithm(Dictionary<string, IAllele> parameters)
        {
            return new SapsRunner(parameters, this._pathToExecutable, this._timeout);
        }

        #endregion
    }
}