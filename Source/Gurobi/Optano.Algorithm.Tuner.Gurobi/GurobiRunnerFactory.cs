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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    /// A simple implementation of <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/>.
    /// </summary>
    public class GurobiRunnerFactory : ITargetAlgorithmFactory<GurobiRunner, InstanceSeedFile, GurobiResult>
    {
        #region Fields

        /// <summary>
        /// The tuner configuration.
        /// </summary>
        private readonly AlgorithmTunerConfiguration _tunerConfiguration;

        /// <summary>
        /// The gurobi runner configuration.
        /// </summary>
        private readonly GurobiRunnerConfiguration _runnerConfiguration;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunnerFactory"/> class.
        /// </summary>
        /// <param name="runnerConfiguration">The <see cref="GurobiRunnerConfiguration"/>.</param>
        /// <param name="tunerConfiguration">The <see cref="AlgorithmTunerConfiguration"/>.</param>
        public GurobiRunnerFactory(GurobiRunnerConfiguration runnerConfiguration, AlgorithmTunerConfiguration tunerConfiguration)

        {
            this._runnerConfiguration = runnerConfiguration;
            this._tunerConfiguration = tunerConfiguration;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Configures a <see cref="GurobiRunner" /> using the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to configure the Gurobi run with.</param>
        /// <returns>The configured <see cref="GurobiRunner" />.</returns>
        public GurobiRunner ConfigureTargetAlgorithm(Dictionary<string, IAllele> parameters)
        {
            // Handle partition place parameters
            parameters = this.CombinePartitionPlaceParameters(parameters);

            // Create a new gurobi environment.
            var gurobiEnvironment = new GRBEnv();

            // Add all simple, non-artificial parameters to it.
            foreach (var param in parameters)
            {
                gurobiEnvironment.Set(param.Key, param.Value.ToString());
            }

            // Log which settings will be used for the next Gurobi run(s).
            LoggingHelper.WriteLine(VerbosityLevel.Info, string.Join(", ", parameters.Select(parameter => $"{parameter.Key}: {parameter.Value}")));

            gurobiEnvironment.Threads = this._runnerConfiguration.ThreadCount;

            gurobiEnvironment.NodefileStart = this._runnerConfiguration.NodefileStartSizeGigabyte;

            if (!Directory.Exists(this._runnerConfiguration.NodefileDirectory.FullName))
            {
                this._runnerConfiguration.NodefileDirectory.Create();
            }

            gurobiEnvironment.NodefileDir = this._runnerConfiguration.NodefileDirectory.FullName;

            // Return the new gurobi runner.
            return new GurobiRunner(gurobiEnvironment, this._runnerConfiguration, this._tunerConfiguration);
        }

        /// <summary>
        /// Tries to get the result from the given string array. This method is the counterpart to <see cref="GurobiResult.ToStringArray"/>.
        /// </summary>
        /// <param name="stringArray">The string array.</param>
        /// <param name="result">The result.</param>
        /// <returns>True, if successful.</returns>
        public bool TryToGetResultFromStringArray(string[] stringArray, out GurobiResult result)
        {
            result = null;

            if (stringArray.Length != 4)
            {
                return false;
            }

            if (!Enum.TryParse(stringArray[0], true, out TargetAlgorithmStatus targetAlgorithmStatus))
            {
                return false;
            }

            if (!double.TryParse(stringArray[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var runtime))
            {
                return false;
            }

            if (!double.TryParse(stringArray[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var gap))
            {
                return false;
            }

            if (!bool.TryParse(stringArray[3], out var hasValidResult))
            {
                return false;
            }

            result = new GurobiResult(
                gap,
                TimeSpan.FromMilliseconds(runtime),
                targetAlgorithmStatus,
                hasValidResult);
            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Combines all values for the separate PartitionPlace bits into a single parameter.
        /// See also <see cref="GurobiUtils.PartitionPlaceGroupIdentifiers"/>.
        /// </summary>
        /// <param name="parameters">The unfiltered parameters.</param>
        /// <returns>The parameters with combined <c>PartitionPlace</c> value.</returns>
        private Dictionary<string, IAllele> CombinePartitionPlaceParameters(Dictionary<string, IAllele> parameters)
        {
            var filteredParameters = parameters.Where(p => !GurobiUtils.PartitionPlaceGroupIdentifiers.Contains(p.Key))
                .ToDictionary(p => p.Key, p => p.Value);

            var partitionPlaceValue = 0;
            foreach (var partitionPlacePart in GurobiUtils.PartitionPlaceGroupIdentifiers)
            {
                if (!parameters.TryGetValue(partitionPlacePart, out var allele))
                {
                    continue;
                }

                partitionPlaceValue += (int)allele.GetValue();
            }

            filteredParameters[GurobiUtils.PartitionPlaceName] = new Allele<int>(partitionPlaceValue);
            return filteredParameters;
        }

        #endregion
    }
}