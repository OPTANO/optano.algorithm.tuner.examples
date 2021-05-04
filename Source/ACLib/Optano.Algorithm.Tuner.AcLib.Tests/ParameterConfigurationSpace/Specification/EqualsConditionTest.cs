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
    /// Contains tests for the <see cref="EqualsCondition"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class EqualsConditionTest : IDisposable
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
        /// Checks that <see cref="EqualsCondition"/>'s constructor throws a <see cref="ArgumentNullException"/> if no
        /// identifier is given.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingIdentifier()
        {
            Assert.Throws<ArgumentNullException>(
                () => new EqualsCondition(
                    identifier: null,
                    allowedValues: new List<IAllele> { new Allele<int>(42) }));
        }

        /// <summary>
        /// Checks that <see cref="EqualsCondition"/>'s constructor throws a <see cref="ArgumentNullException"/> if no
        /// value set is given.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingValues()
        {
            Assert.Throws<ArgumentNullException>(() => new EqualsCondition("identifier", allowedValues: null));
        }

        /// <summary>
        /// Checks that <see cref="EqualsCondition"/>'s constructor throws a <see cref="ArgumentOutOfRangeException"/>
        /// if the given value set is empty.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForZeroValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EqualsCondition("identifier", allowedValues: new List<IAllele>()));
        }

        /// <summary>
        /// Checks that <see cref="EqualsCondition.IsMet"/> works correctly for strings.
        /// </summary>
        [Fact]
        public void IsMetWorksForString()
        {
            var condition = new EqualsCondition(
                "string_1",
                new List<IAllele> { new Allele<string>("a"), new Allele<string>("foo") });

            this._parameters.Add("string_0", new Allele<string>("a"));
            this._parameters.Add("string_1", new Allele<string>("foo"));
            Assert.True(condition.IsMet(this._parameters), "Expected condition to be met.");

            this._parameters["string_1"] = new Allele<string>("bar");
            Assert.False(condition.IsMet(this._parameters), "Changed parameter should not meet condition anymore.");

            this._parameters.Remove("string_1");
            Assert.False(
                condition.IsMet(this._parameters),
                "Condition should not be met if the parameter is not set at all.");
        }

        /// <summary>
        /// Checks that <see cref="EqualsCondition.IsMet"/> works correctly for integers.
        /// </summary>
        [Fact]
        public void IsMetWorksForInteger()
        {
            var condition = new EqualsCondition(
                "int_1",
                new List<IAllele> { new Allele<int>(-234), new Allele<int>(12) });

            this._parameters.Add("int_0", new Allele<int>(12));
            this._parameters.Add("int_1", new Allele<int>(-234));
            Assert.True(condition.IsMet(this._parameters), "Expected condition to be met.");

            this._parameters["int_1"] = new Allele<int>(234);
            Assert.False(condition.IsMet(this._parameters), "Changed parameter should not meet condition anymore.");

            this._parameters.Remove("int_1");
            Assert.False(
                condition.IsMet(this._parameters),
                "Condition should not be met if the parameter is not set at all.");
        }

        #endregion
    }
}