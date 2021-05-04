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

namespace Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    /// <summary>
    /// A <see cref="GenomeBuilder"/> using a <see cref="ParameterConfigurationSpaceSpecification"/> to check validity.
    /// </summary>
    public class ParameterConfigurationSpaceGenomeBuilder : GenomeBuilder
    {
        #region Fields

        /// <summary>
        /// A specification defining forbidden parameter combinations and activity conditions.
        /// </summary>
        private readonly ParameterConfigurationSpaceSpecification _parameterSpecification;

        /// <summary>
        /// A convenient mapping from identifiers to <see cref="IParameterNode"/>s.
        /// </summary>
        private readonly Dictionary<string, IParameterNode> _identifierToTreeNode;

        /// <summary>
        /// The parameters' structure.
        /// </summary>
        private readonly ParameterTree _parameterTree;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfigurationSpaceGenomeBuilder"/> class.
        /// </summary>
        /// <param name="parameterTree">The parameters' structure.</param>
        /// <param name="parameterSpecification">
        /// A specification defining forbidden parameter combinations and activity conditions.
        /// </param>
        /// <param name="configuration">Configuration parameters.</param>
        public ParameterConfigurationSpaceGenomeBuilder(
            ParameterTree parameterTree,
            ParameterConfigurationSpaceSpecification parameterSpecification,
            AlgorithmTunerConfiguration configuration)
            : base(parameterTree, configuration)
        {
            this._parameterTree = parameterTree;
            this._parameterSpecification = parameterSpecification ??
                                           throw new ArgumentNullException(nameof(parameterSpecification));
            this._identifierToTreeNode =
                this._parameterTree.GetParameters().ToDictionary(node => node.Identifier, node => node);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Decides whether the given <see cref="Genome"/> is valid.
        /// </summary>
        /// <param name="genome">The <see cref="Genome"/> to test.</param>
        /// <returns>False if the <see cref="Genome"/> is invalid.</returns>
        public override bool IsGenomeValid(Genome genome)
        {
            if (!base.IsGenomeValid(genome))
            {
                return false;
            }

            return !this.TryFindForbiddenCombination(genome, out var dummy);
        }

        /// <summary>
        /// Tries to make the given genome valid by using <see cref="GenomeBuilder.MutateParameter" /> on parameters
        /// which are responsible.
        /// </summary>
        /// <param name="genome">Genome to make valid. Will be modified.</param>
        /// <exception cref="TimeoutException">Thrown if genome could not be made valid.</exception>
        public override void MakeGenomeValid(Genome genome)
        {
            int repairAttempts = 0;
            while (this.TryFindForbiddenCombination(genome, out var forbiddenCombination))
            {
                if (repairAttempts == 0)
                {
                    LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Repairing genome {genome}.");
                }

                repairAttempts++;
                if (repairAttempts > this.MaximumRepairAttempts)
                {
                    throw new TimeoutException(
                        $"Tried to make the genome {genome} valid by mutating parameters in forbidden combinations {this.MaximumRepairAttempts} times, but failed. Current forbidden combination: {forbiddenCombination}.");
                }

                var randomCombinationPartIdentifier = Randomizer.Instance.ChooseRandomSubset(forbiddenCombination.ParameterIdentifiers, 1).Single();
                this.MutateParameter(genome, this._identifierToTreeNode[randomCombinationPartIdentifier]);
            }

            if (repairAttempts > 0)
            {
                LoggingHelper.WriteLine(
                    VerbosityLevel.Debug,
                    $"Repaired forbidden combinations in genome, now {genome}.");
            }

            base.MakeGenomeValid(genome);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries the find a <see cref="ForbiddenParameterCombination"/> which is met.
        /// </summary>
        /// <param name="genome">The genome specifying the parameter values.</param>
        /// <param name="combination">The found combination. May be <c>null</c>.</param>
        /// <returns><c>true</c> if a combination was found.</returns>
        private bool TryFindForbiddenCombination(Genome genome, out ForbiddenParameterCombination combination)
        {
            // Only check relevant rules, i.e. rules on active parameters.
            var activeParameters =
                this._parameterSpecification.ExtractActiveParameters(genome.GetFilteredGenes(this._parameterTree));

            combination = this._parameterSpecification.ForbiddenParameterCombinations
                .FirstOrDefault(rule => rule.IsMet(activeParameters));
            return combination != null;
        }

        #endregion
    }
}