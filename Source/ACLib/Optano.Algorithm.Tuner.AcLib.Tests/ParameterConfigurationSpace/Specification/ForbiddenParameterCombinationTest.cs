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

namespace Optano.Algorithm.Tuner.AcLib.Tests.ParameterConfigurationSpace.Specification
{
    using System;
    using System.Collections.Generic;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Genomes.Values;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ForbiddenParameterCombination"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ForbiddenParameterCombinationTest : IDisposable
    {
        #region Fields

        /// <summary>
        /// Some parameter settings.
        /// </summary>
        private readonly Dictionary<string, IAllele> _parameters = new Dictionary<string, IAllele>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Called after every tests.
        /// </summary>
        public void Dispose()
        {
            this._parameters.Clear();
        }

        /// <summary>
        /// Checks that <see cref="ForbiddenParameterCombination"/>'s constructor throws a
        /// <see cref="ArgumentNullException"/> when called without specifying a combination.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingSpecification()
        {
            Assert.Throws<ArgumentNullException>(() => new ForbiddenParameterCombination(combination: null));
        }

        /// <summary>
        /// Checks that <see cref="ForbiddenParameterCombination"/>'s constructor throws a
        /// <see cref="ArgumentOutOfRangeException"/> when called without any conditions in the specification.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForEmptySpecification()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ForbiddenParameterCombination(new Dictionary<string, IAllele>()));
        }

        /// <summary>
        /// Checks that <see cref="ForbiddenParameterCombination.IsMet"/> returns <c>true</c> if all conditions
        /// are fulfilled.
        /// </summary>
        [Fact]
        public void IsMetReturnsTrueForForbiddenCombination()
        {
            var forbiddenParameters = new Dictionary<string, IAllele>
                                          {
                                              { "a", new Allele<int>(3) },
                                              { "b", new Allele<string>("bad") },
                                          };
            var combination = new ForbiddenParameterCombination(forbiddenParameters);

            this._parameters.Add("c", new Allele<int>(3));
            this._parameters.Add("a", new Allele<int>(3));
            this._parameters.Add("b", new Allele<string>("bad"));

            Assert.True(combination.IsMet(this._parameters), "Expected the condition to be met.");
        }

        /// <summary>
        /// Checks that <see cref="ForbiddenParameterCombination.IsMet"/> returns <c>false</c> if any conditions
        /// is not met.
        /// </summary>
        [Fact]
        public void IsMetReturnsFalseForSingleDifferingParameter()
        {
            var forbiddenParameters = new Dictionary<string, IAllele>
                                          {
                                              { "a", new Allele<int>(3) },
                                              { "b", new Allele<string>("bad") },
                                          };
            var combination = new ForbiddenParameterCombination(forbiddenParameters);

            this._parameters.Add("c", new Allele<int>(3));
            this._parameters.Add("a", new Allele<int>(2));
            this._parameters.Add("b", new Allele<string>("bad"));

            Assert.False(combination.IsMet(this._parameters), "Condition should not be met; 'a' is different.");
        }

        /// <summary>
        /// Checks that <see cref="ForbiddenParameterCombination.IsMet"/> returns <c>false</c> if one of the 
        /// parameters is not set at all.
        /// </summary>
        [Fact]
        public void IsMetReturnsFalseForMissingParameter()
        {
            var forbiddenParameters = new Dictionary<string, IAllele>
                                          {
                                              { "a", new Allele<int>(3) },
                                              { "b", new Allele<string>("bad") },
                                          };
            var combination = new ForbiddenParameterCombination(forbiddenParameters);

            this._parameters.Add("b", new Allele<string>("bad"));

            Assert.False(combination.IsMet(this._parameters), "Condition should not be met without 'b'.");
        }

        /// <summary>
        /// Checks that <see cref="ForbiddenParameterCombination.ToString"/> is of the form "{a=3, b=hello}".
        /// </summary>
        [Fact]
        public void ToStringDescribesCompleteCombination()
        {
            var forbiddenParameters = new Dictionary<string, IAllele>
                                          {
                                              { "a", new Allele<int>(3) },
                                              { "b", new Allele<string>("bad") },
                                          };
            var combination = new ForbiddenParameterCombination(forbiddenParameters);

            Assert.Equal("{a=3, b=bad}", combination.ToString());
        }

        #endregion
    }
}