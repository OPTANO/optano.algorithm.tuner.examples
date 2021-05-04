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

namespace Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    /// <summary>
    /// The parsed presentation of a file in Parameter Configuration Space (PCS) format.
    /// See http://www.cs.ubc.ca/labs/beta/Projects/SMAC/v2.08.00/manual.pdf#page=12.
    /// </summary>
    public class ParameterConfigurationSpaceSpecification
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfigurationSpaceSpecification"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="parameterActivityConditions">
        /// A mapping from parameter identifiers to conditions determining whether it is active.
        /// </param>
        /// <param name="forbiddenParameterCombinations">Forbidden parameter combinations.</param>
        public ParameterConfigurationSpaceSpecification(
            IEnumerable<IParameterNode> parameters,
            Dictionary<string, List<EqualsCondition>> parameterActivityConditions,
            IEnumerable<ForbiddenParameterCombination> forbiddenParameterCombinations)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameterActivityConditions == null)
            {
                throw new ArgumentNullException(nameof(parameterActivityConditions));
            }

            if (forbiddenParameterCombinations == null)
            {
                throw new ArgumentNullException(nameof(forbiddenParameterCombinations));
            }

            this.Parameters = parameters.ToImmutableList();
            if (!this.Parameters.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(parameters), "No parameters have been given.");
            }

            this.ParameterActivityConditions = parameterActivityConditions.ToImmutableDictionary(
                keyAndValue => keyAndValue.Key,
                keyAndValue => keyAndValue.Value.ToImmutableList());
            this.ForbiddenParameterCombinations = forbiddenParameterCombinations.ToImmutableList();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the defined parameters.
        /// </summary>
        public ImmutableList<IParameterNode> Parameters { get; }

        /// <summary>
        /// Gets all forbidden parameter combinations.
        /// </summary>
        public ImmutableList<ForbiddenParameterCombination> ForbiddenParameterCombinations { get; }

        /// <summary>
        /// Gets a mapping from parameter identifiers to conditions determining whether it is active / will be given to the
        /// target algorithm.
        /// </summary>
        public ImmutableDictionary<string, ImmutableList<EqualsCondition>> ParameterActivityConditions { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Extracts the active parameters as specified by <see cref="ParameterActivityConditions"/>.
        /// </summary>
        /// <param name="parameters">All parameters.</param>
        /// <returns>The active parameters.</returns>
        public Dictionary<string, IAllele> ExtractActiveParameters(Dictionary<string, IAllele> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var filteredParameters = new Dictionary<string, IAllele>(parameters);
            foreach (var parameterToConditions in this.ParameterActivityConditions)
            {
                if (parameterToConditions.Value.Any(condition => !condition.IsMet(parameters)))
                {
                    filteredParameters.Remove(parameterToConditions.Key);
                }
            }

            return filteredParameters;
        }

        #endregion
    }
}