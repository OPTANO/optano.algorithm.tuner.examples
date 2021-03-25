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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    /// A simple implementation of <see cref="ITargetAlgorithmFactory{TTargetAlgorithm,TInstance,TResult}"/>.
    /// </summary>
    public class GurobiRunnerFactory : ITargetAlgorithmFactory<GurobiRunner, InstanceSeedFile, GurobiResult>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunnerFactory"/> class.
        /// </summary>
        /// <param name="gurobiSettings">The gurobi settings.</param>
        public GurobiRunnerFactory(GurobiRunnerConfiguration gurobiSettings)

        {
            this.GurobiSettings = gurobiSettings;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the configuration of the Gurobi runner.
        /// </summary>
        private GurobiRunnerConfiguration GurobiSettings { get; set; }

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

            gurobiEnvironment.Threads = this.GurobiSettings.ThreadCount;

            gurobiEnvironment.NodefileStart = this.GurobiSettings.NodefileStartSizeGigabyte;

            if (!Directory.Exists(this.GurobiSettings.NodefileDirectory.FullName))
            {
                this.GurobiSettings.NodefileDirectory.Create();
            }

            gurobiEnvironment.NodefileDir = this.GurobiSettings.NodefileDirectory.FullName;

            // Return the new gurobi runner.
            return new GurobiRunner(gurobiEnvironment, this.GurobiSettings);
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