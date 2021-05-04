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

namespace Optano.Algorithm.Tuner.AcLib.Tests.ParameterConfigurationSpace
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace;
    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ParameterConfigurationSpaceConverter"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ParameterConfigurationSpaceConverterTest : IDisposable
    {
        #region Constants

        /// <summary>
        /// Path for PCS file.
        /// </summary>
        private const string ParameterConfigurationSpaceFile = "test.pcs";

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
            if (File.Exists(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile))
            {
                File.Delete(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile);
            }
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> can correctly convert a
        /// fully-featured PCS file.
        /// </summary>
        [Fact]
        public void ConvertCorrectlyConvertsFromCorrectFormat()
        {
            const string PathToFullFeaturedParameterConfigurationSpaceFile = @"Tools/fullFeaturedParameterConfigurationSpace.pcs";

            var specification =
                ParameterConfigurationSpaceConverter.Convert(PathToFullFeaturedParameterConfigurationSpaceFile);

            // Check parameters.
            var parameters = specification.Parameters;
            var expectedParameters = new List<IParameterNode>
                                         {
                                             new ValueNode<string>(
                                                 "@1:some-category",
                                                 new CategoricalDomain<string>(new List<string> { "0", "1", "2", "3" })),
                                             new ValueNode<int>("@1:discrete-log", new DiscreteLogDomain(1, 1024)),
                                             new ValueNode<int>("@1:discrete", new IntegerDomain(75, 99)),
                                             new ValueNode<double>("@1:2:continuous", new ContinuousDomain(-1.02, 2.6)),
                                             new ValueNode<double>("@1:2:logarithmic", new LogDomain(29.4, 200.8)),
                                             new ValueNode<string>(
                                                 "@1:0:complicated-category",
                                                 new CategoricalDomain<string>(new List<string> { "F", "L", "x", "+", "no" })),
                                             new ValueNode<int>("@1:5:A:discrete-log-with-spaces", new DiscreteLogDomain(1, 65535)),
                                             new ValueNode<int>("@0:4:discrete-with-spaces", new IntegerDomain(0, 100)),
                                             new ValueNode<double>("@1:2:G:continuous-with-spaces", new ContinuousDomain(1.34, 2.5)),
                                             new ValueNode<double>("@1:2:G:logarithmic-with-spaces", new LogDomain(0.5, 3.0)),
                                         };

            Assert.Equal(expectedParameters.Count, parameters.Count);
            foreach (var expected in expectedParameters)
            {
                var parameter = parameters.FirstOrDefault(p => object.Equals(p.Identifier, expected.Identifier));

                Assert.NotNull(parameter);
                ParameterConfigurationSpaceConverterTest.CheckDomainEquality(expected, parameter);
            }

            // Check forbidden combinations.
            Assert.Equal(
                2,
                specification.ForbiddenParameterCombinations.Count);
            var forbiddenCombination1 = new Dictionary<string, IAllele>
                                            {
                                                { "@1:2:G:logarithmic-with-spaces", new Allele<double>(1) },
                                                { "@1:2:continuous", new Allele<double>(1.5) },
                                                { "@1:some-category", new Allele<string>("3") },
                                            };
            ParameterConfigurationSpaceConverterTest.CheckForbiddenCombination(
                forbiddenCombination1,
                specification.ForbiddenParameterCombinations[0]);
            var forbiddenCombination2 = new Dictionary<string, IAllele>
                                            {
                                                { "@1:discrete", new Allele<int>(76) },
                                                { "@1:0:complicated-category", new Allele<string>("+") },
                                            };
            ParameterConfigurationSpaceConverterTest.CheckForbiddenCombination(
                forbiddenCombination2,
                specification.ForbiddenParameterCombinations[1]);

            // Check conditions.
            Assert.True(
                specification.ParameterActivityConditions.ContainsKey("@1:2:continuous"),
                "Expected '@1:2:continuous' to have conditions.");
            Assert.Equal(
                2,
                specification.ParameterActivityConditions["@1:2:continuous"].Count);
            ParameterConfigurationSpaceConverterTest.CheckAllowedValues(
                specification.ParameterActivityConditions["@1:2:continuous"][0],
                "@1:0:complicated-category",
                new IAllele[] { new Allele<string>("+"), new Allele<string>("F"), new Allele<string>("x") });
            ParameterConfigurationSpaceConverterTest.CheckAllowedValues(
                specification.ParameterActivityConditions["@1:2:continuous"][1],
                "@1:discrete",
                new IAllele[] { new Allele<int>(75) });
            Assert.True(
                specification.ParameterActivityConditions.ContainsKey("@1:5:A:discrete-log-with-spaces"),
                "Expected '@1:5:A:discrete-log-with-spaces' to have conditions.");
            Assert.Single(
                specification.ParameterActivityConditions["@1:5:A:discrete-log-with-spaces"]);
            ParameterConfigurationSpaceConverterTest.CheckAllowedValues(
                specification.ParameterActivityConditions["@1:5:A:discrete-log-with-spaces"][0],
                "@1:some-category",
                new IAllele[] { new Allele<string>("0"), new Allele<string>("2") });
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="FormatException"/> if encountering a line of unknown format.
        /// </summary>
        [Fact]
        public void ConvertThrowsForUnknownLineFormat()
        {
            // Create PCS file with wrong bracketing.
            this.WritePcsFile(new[] { "factor [2, 15} [5]i" });
            Assert.Throws<FormatException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="InvalidOperationException"/> if encountering a condition for an undefined parameter.
        /// </summary>
        [Fact]
        public void ConvertThrowsForConditionUsingUnknownChild()
        {
            this.WritePcsFile(
                new[]
                    {
                        "sort-algo {quick,insertion,merge,heap} [quick]",
                        "quick-selection-method | sort-algo in {quick}",
                    });
            Assert.Throws<InvalidOperationException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="InvalidOperationException"/> if encountering a condition on an undefined parameter.
        /// </summary>
        [Fact]
        public void ConvertThrowsForConditionUsingUnknownParent()
        {
            this.WritePcsFile(
                new[]
                    {
                        "quick-selection-method { first, random, median-of-medians} [random]",
                        "quick-selection-method | sort-algo in {quick}",
                    });
            Assert.Throws<InvalidOperationException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="FormatException"/> if encountering a condition using values not fitting the parameter's domain.
        /// </summary>
        [Fact]
        public void ConvertThrowsForConditionUsingWrongDomain()
        {
            this.WritePcsFile(
                new[]
                    {
                        "factor [2, 15] [5]i",
                        "value [0.2, 45.91] [5.3]",
                        "value | factor in {five}",
                    });
            Assert.Throws<FormatException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="InvalidOperationException"/> if encountering a forbidden parameter clause using 
        /// an undefined parameter.
        /// </summary>
        [Fact]
        public void ConvertThrowsForForbiddenCombinationUsingUnknownParameter()
        {
            this.WritePcsFile(
                new[]
                    {
                        "factor [2, 15] [5]i",
                        "{value=5}",
                    });
            Assert.Throws<InvalidOperationException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="FormatException"/> if encountering a forbidden parameter cluase using values not fitting a
        /// parameter's domain.
        /// </summary>
        [Fact]
        public void ConvertThrowsForForbiddenCombinationUsingWrongDomain()
        {
            this.WritePcsFile(
                new[]
                    {
                        "factor [2, 15] [5]i",
                        "{factor=3.4}",
                    });
            Assert.Throws<FormatException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="FormatException"/> if encountering a definition of an integer parameter with a continuous
        /// minimum value.
        /// </summary>
        [Fact]
        public void ConvertThrowsForContinuousMinValueForDiscreteParameter()
        {
            this.WritePcsFile(new[] { "factor [2.1, 15] [5]i" });
            Assert.Throws<FormatException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceConverter.Convert"/> throws a
        /// <see cref="FormatException"/> if encountering a definition of an integer parameter with a continuous
        /// maximum value.
        /// </summary>
        [Fact]
        public void ConvertThrowsForContinuousMaxValueForDiscreteParameter()
        {
            this.WritePcsFile(new[] { "factor [2, 15.06] [5]i" });
            Assert.Throws<FormatException>(
                () => ParameterConfigurationSpaceConverter.Convert(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks that the parameter's <see cref="IDomain"/>s are equivalent.
        /// </summary>
        /// <param name="expected">A <see cref="IParameterNode"/> with expected domain.</param>
        /// <param name="parameter">A <see cref="IParameterNode"/> with the actual domain.</param>
        private static void CheckDomainEquality(IParameterNode expected, IParameterNode parameter)
        {
            Assert.Equal(expected.Domain.GetType(), parameter.Domain.GetType());

            switch (parameter.Domain)
            {
                case CategoricalDomain<string> categoricalDomain:
                    Assert.Equal(
                        ((CategoricalDomain<string>)expected.Domain).PossibleValues.ToArray(),
                        categoricalDomain.PossibleValues.ToArray());
                    break;
                case NumericalDomain<double> continuousDomain:
                    Assert.Equal(
                        ((NumericalDomain<double>)expected.Domain).Minimum,
                        continuousDomain.Minimum);
                    Assert.Equal(
                        ((NumericalDomain<double>)expected.Domain).Maximum,
                        continuousDomain.Maximum);
                    break;
                case NumericalDomain<int> discreteDomain:
                    Assert.Equal(
                        ((NumericalDomain<int>)expected.Domain).Minimum,
                        discreteDomain.Minimum);
                    Assert.Equal(
                        ((NumericalDomain<int>)expected.Domain).Maximum,
                        discreteDomain.Maximum);
                    break;
                default:
                    throw new NotSupportedException(
                        $"All domains should either be categorical strings, continuous or discrete, but {parameter.Identifier}'s domain is of type {parameter.Domain.GetType()}.");
            }
        }

        /// <summary>
        /// Checks whether a <see cref="ForbiddenParameterCombination"/> is as expected.
        /// </summary>
        /// <param name="expectedCombination">The expected combination.</param>
        /// <param name="actual">The actual combination.</param>
        private static void CheckForbiddenCombination(Dictionary<string, IAllele> expectedCombination, ForbiddenParameterCombination actual)
        {
            Assert.True(actual.IsMet(expectedCombination), "Combination is too strict.");
            foreach (var definition in expectedCombination)
            {
                var subset = new Dictionary<string, IAllele>(expectedCombination);
                subset.Remove(definition.Key);
                Assert.False(
                    actual.IsMet(subset),
                    $"Combination is not strict enough: Is also met without {definition.Key}.");
            }
        }

        /// <summary>
        /// Checks that the specified values are allowed by the condition.
        /// </summary>
        /// <param name="actual">The condition.</param>
        /// <param name="identifier">The relevant parameter's identifier.</param>
        /// <param name="allowedValues">The values to check.</param>
        private static void CheckAllowedValues(EqualsCondition actual, string identifier, IAllele[] allowedValues)
        {
            foreach (var value in allowedValues)
            {
                Assert.True(
                    actual.IsMet(new Dictionary<string, IAllele> { { identifier, value } }),
                    $"Condition is not met for '{identifier}' set to {value}.");
            }
        }

        /// <summary>
        /// Writes a PCS file at <see cref="ParameterConfigurationSpaceFile"/>.
        /// </summary>
        /// <param name="lines">The lines to write.</param>
        private void WritePcsFile(IEnumerable<string> lines)
        {
            using var file = File.CreateText(ParameterConfigurationSpaceConverterTest.ParameterConfigurationSpaceFile);
            foreach (var line in lines)
            {
                file.WriteLine(line);
            }
        }

        #endregion
    }
}