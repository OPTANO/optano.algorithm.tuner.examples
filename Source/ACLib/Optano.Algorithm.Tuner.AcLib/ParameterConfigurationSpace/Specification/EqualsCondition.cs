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

    using Optano.Algorithm.Tuner.Genomes.Values;

    /// <summary>
    /// A condition on a parameter to be in a certain set of values.
    /// </summary>
    public class EqualsCondition
    {
        #region Fields

        /// <summary>
        /// The identifier of the considered parameter.
        /// </summary>
        private readonly string _identifier;

        /// <summary>
        /// The restricted set of values <see cref="_identifier"/> should be in for this condition to be met.
        /// </summary>
        private readonly List<IAllele> _allowedValues;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsCondition"/> class.
        /// </summary>
        /// <param name="identifier">The identifier of the considered parameter.</param>
        /// <param name="allowedValues">The allowed values.</param>
        public EqualsCondition(string identifier, IEnumerable<IAllele> allowedValues)
        {
            this._identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this._allowedValues = allowedValues?.ToList() ?? throw new ArgumentNullException(nameof(allowedValues));

            if (!this._allowedValues.Any())
            {
                throw new ArgumentOutOfRangeException(
                    nameof(allowedValues),
                    $"There are no allowed values for '{identifier}'. Condition can never be fulfilled.");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Determines whether the specified condition is met.
        /// </summary>
        /// <param name="parameters">The parameters with their values.</param>
        /// <returns>
        ///   <c>true</c> if the specified condition is met; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMet(Dictionary<string, IAllele> parameters)
        {
            return parameters.ContainsKey(this._identifier)
                   && this._allowedValues.Any(value => Equals(value, parameters[this._identifier]));
        }

        #endregion
    }
}