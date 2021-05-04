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
    using System.Linq;
    using System.Text;

    using Optano.Algorithm.Tuner.Genomes.Values;

    /// <summary>
    /// An invalid combination of parameter values.
    /// </summary>
    public class ForbiddenParameterCombination
    {
        #region Fields

        /// <summary>
        /// The forbidden combination of parameter values.
        /// </summary>
        private readonly Dictionary<string, IAllele> _combination;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenParameterCombination"/> class.
        /// </summary>
        /// <param name="combination">The forbidden parameter combination.</param>
        public ForbiddenParameterCombination(Dictionary<string, IAllele> combination)
        {
            if (combination == null)
            {
                throw new ArgumentNullException(nameof(combination));
            }

            if (!combination.Any())
            {
                throw new ArgumentOutOfRangeException(
                    nameof(combination),
                    "Tried to create a forbidden parameter clause without any parameters!");
            }

            this._combination = combination;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets parameter identifiers contained in the combination.
        /// </summary>
        public IEnumerable<string> ParameterIdentifiers => this._combination.Keys;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Determines whether the specified forbidden combination is active.
        /// </summary>
        /// <param name="parameters">The parameters with their values.</param>
        /// <returns>
        ///   <c>true</c> if the specified forbidden combination is active; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMet(Dictionary<string, IAllele> parameters)
        {
            return this._combination.All(
                condition =>
                    parameters.TryGetValue(condition.Key, out var value) && Equals(value, condition.Value));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var description = new StringBuilder("{");
            foreach (var condition in this._combination)
            {
                description.Append($"{condition.Key}={condition.Value}, ");
            }

            // Remove last ", "
            description.Remove(description.Length - 2, 2);
            description.Append("}");

            return description.ToString();
        }

        #endregion
    }
}