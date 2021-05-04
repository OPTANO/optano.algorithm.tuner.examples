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

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace;
    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ParameterConfigurationSpaceGenomeBuilder"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ParameterConfigurationSpaceGenomeBuilderTest : IDisposable
    {
        #region Constants

        /// <summary>
        /// Identifier of an unconditional parameter.
        /// </summary>
        private const string UnconditionalParameter = "unconditional";

        /// <summary>
        /// Identifier of a conditional parameter.
        /// </summary>
        private const string ConditionalParameter = "conditional";

        #endregion

        #region Fields

        /// <summary>
        /// A simple <see cref="AlgorithmTunerConfiguration"/> object.
        /// </summary>
        private readonly AlgorithmTunerConfiguration _configuration
            = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().Build(1);

        /// <summary>
        /// A parameter specification with one parameter named <see cref="UnconditionalParameter"/> and one named
        /// <see cref="ConditionalParameter"/>.
        /// <see cref="ConditionalParameter"/> is only active if <see cref="UnconditionalParameter"/> is 20.
        /// <see cref="ConditionalParameter"/> may not be 0.
        /// </summary>
        private readonly ParameterConfigurationSpaceSpecification _parameterSpecification;

        /// <summary>
        /// A <see cref="ParameterTree"/> matching <see cref="_parameterSpecification"/>.
        /// </summary>
        private readonly ParameterTree _parameterTree;

        /// <summary>
        /// The <see cref="ParameterConfigurationSpaceGenomeBuilder"/> initialized with <see cref="_parameterTree"/>,
        /// <see cref="_parameterSpecification"/> and <see cref="_configuration"/>.
        /// </summary>
        private ParameterConfigurationSpaceGenomeBuilder _genomeBuilder;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfigurationSpaceGenomeBuilderTest"/> class.
        /// </summary>
        public ParameterConfigurationSpaceGenomeBuilderTest()
        {
            this._parameterSpecification = ParameterConfigurationSpaceGenomeBuilderTest.CreateParameterConfigurationSpaceSpecification();
            this._parameterTree = AcLibUtils.CreateParameterTree(this._parameterSpecification);
            this._genomeBuilder = new ParameterConfigurationSpaceGenomeBuilder(
                this._parameterTree,
                this._parameterSpecification,
                this._configuration);

            Randomizer.Reset();
            Randomizer.Configure();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Called after every test.
        /// </summary>
        public void Dispose()
        {
            Randomizer.Reset();
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder"/>'s constructor throws a
        /// <see cref="ArgumentNullException"/> if called without a <see cref="ParameterTree"/>.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingParameterTree()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ParameterConfigurationSpaceGenomeBuilder(
                    parameterTree: null,
                    parameterSpecification: this._parameterSpecification,
                    configuration: this._configuration));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder"/>'s constructor throws a
        /// <see cref="ArgumentNullException"/> if called without a <see cref="AlgorithmTunerConfiguration"/>.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingConfiguration()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ParameterConfigurationSpaceGenomeBuilder(
                    this._parameterTree,
                    this._parameterSpecification,
                    configuration: null));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder"/>'s constructor throws a
        /// <see cref="ArgumentNullException"/> if called without a
        /// <see cref="ParameterConfigurationSpaceSpecification"/>.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingParameterConfigurationSpaceSpecification()
        {
            Assert.Throws<ArgumentNullException>(
                () => new ParameterConfigurationSpaceGenomeBuilder(
                    this._parameterTree,
                    parameterSpecification: null,
                    configuration: this._configuration));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.IsGenomeValid"/> returns <c>false</c> if
        /// one of the genes has an incorrect value type.
        /// </summary>
        [Fact]
        public void IsGenomeValidReturnsFalseForWrongValueType()
        {
            var genome = new Genome();
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter, new Allele<int>(-30));
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter, new Allele<double>(3));
            Assert.False(
                this._genomeBuilder.IsGenomeValid(genome),
                $"Genome should not be valid because {ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter} is an integer parameter with a continuous allele.");
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.IsGenomeValid"/> returns <c>false</c> if
        /// one of the genes is out of its domain bounds.
        /// </summary>
        [Fact]
        public void IsGenomeValidReturnsFalseForOutOfDomain()
        {
            var genome = new Genome();
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter, new Allele<int>(-30));
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter, new Allele<int>(5));
            Assert.False(
                this._genomeBuilder.IsGenomeValid(genome),
                $"Genome should not be valid because {ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter} has a value outside of its domain.");
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.IsGenomeValid"/> returns <c>false</c> if
        /// a <see cref="ForbiddenParameterCombination"/> is triggered.
        /// </summary>
        [Fact]
        public void IsGenomeValidReturnsFalseForForbiddenCombination()
        {
            var genome = new Genome();
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter, new Allele<int>(20));
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter, new Allele<int>(0));
            Assert.False(
                this._genomeBuilder.IsGenomeValid(genome),
                $"Genome should not be valid because {ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter} has a forbidden value.");
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.IsGenomeValid"/> does not return
        /// <c>false</c> if a <see cref="ForbiddenParameterCombination"/> would be triggered by an inactive gene only.
        /// </summary>
        [Fact]
        public void IsGenomeValidIgnoresInactiveParametersForForbiddenCombinations()
        {
            var genome = new Genome();
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter, new Allele<int>(-30));
            genome.SetGene(ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter, new Allele<int>(0));
            Assert.True(
                this._genomeBuilder.IsGenomeValid(genome),
                $"Genome should be valid because {ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter} with forbidden value is inactive.");
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.MakeGenomeValid"/> only changes 
        /// parameters involved in active <see cref="ForbiddenParameterCombination"/>s.
        /// </summary>
        [Fact]
        public void MakeGenomeValidOnlyChangesRelevantParameters()
        {
            // Create a specification with several parameters.
            var parameters = new List<IParameterNode>
                                 {
                                     new ValueNode<int>("a", new IntegerDomain(-2, 4)),
                                     new ValueNode<int>("Forbidden", new IntegerDomain(-30, 60)),
                                     new ValueNode<int>("c", new IntegerDomain(-2, 4)),
                                 };
            var forbiddenValue = new Dictionary<string, IAllele> { { "Forbidden", new Allele<int>(0) } };
            var inactiveForbiddenValue = new Dictionary<string, IAllele> { { "a", new Allele<int>(2) } };
            var forbiddenCombinations = new List<ForbiddenParameterCombination>
                                            {
                                                new ForbiddenParameterCombination(inactiveForbiddenValue),
                                                new ForbiddenParameterCombination(forbiddenValue),
                                            };
            var multipleParameterSpecification = new ParameterConfigurationSpaceSpecification(
                parameters,
                new Dictionary<string, List<EqualsCondition>>(),
                forbiddenCombinations);
            this._genomeBuilder = new ParameterConfigurationSpaceGenomeBuilder(
                AcLibUtils.CreateParameterTree(multipleParameterSpecification),
                multipleParameterSpecification,
                this._configuration);

            // Several times:
            int numberTests = 20;
            for (int i = 0; i < numberTests; i++)
            {
                // Create an invalid genome.
                var genome = new Genome();
                genome.SetGene("a", new Allele<int>(-1));
                genome.SetGene("Forbidden", new Allele<int>(0));
                genome.SetGene("c", new Allele<int>(2));

                // Repair it.
                this._genomeBuilder.MakeGenomeValid(genome);

                // Check it is repaired with only the forbidden value changed.
                Assert.NotEqual(
                    0,
                    genome.GetGeneValue("Forbidden").GetValue());
                Assert.Equal(-1, genome.GetGeneValue("a").GetValue());
                Assert.Equal(2, genome.GetGeneValue("c").GetValue());
            }
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.MakeGenomeValid"/> can handle multiple,
        /// dependent rules.
        /// </summary>
        [Fact]
        public void MakeGenomeValidCanHandleMultipleIssues()
        {
            // Create a specification with dependent rules.
            var parameters = new List<IParameterNode>
                                 {
                                     new ValueNode<int>("a", new IntegerDomain(-2, 4)),
                                     new ValueNode<int>("Forbidden", new CategoricalDomain<int>(new List<int> { 6, 89 })),
                                     new ValueNode<int>("c", new IntegerDomain(-2, 4)),
                                 };
            var forbiddenValue = new Dictionary<string, IAllele> { { "Forbidden", new Allele<int>(6) } };
            var dependentRule = new Dictionary<string, IAllele> { { "a", new Allele<int>(2) }, { "Forbidden", new Allele<int>(89) } };
            var forbiddenCombinations = new List<ForbiddenParameterCombination>
                                            {
                                                new ForbiddenParameterCombination(dependentRule),
                                                new ForbiddenParameterCombination(forbiddenValue),
                                            };
            var multipleParameterSpecification = new ParameterConfigurationSpaceSpecification(
                parameters,
                new Dictionary<string, List<EqualsCondition>>(),
                forbiddenCombinations);
            this._genomeBuilder = new ParameterConfigurationSpaceGenomeBuilder(
                AcLibUtils.CreateParameterTree(multipleParameterSpecification),
                multipleParameterSpecification,
                this._configuration);

            // Create an genome which is invalid and needs at least two mutations to get valid.
            var genome = new Genome();
            genome.SetGene("a", new Allele<int>(2));
            genome.SetGene("Forbidden", new Allele<int>(6));
            genome.SetGene("c", new Allele<int>(-1));

            this._genomeBuilder.MakeGenomeValid(genome);

            Assert.Equal(89, genome.GetGeneValue("Forbidden").GetValue());
            Assert.NotEqual(2, genome.GetGeneValue("a").GetValue());
            Assert.Equal(-1, genome.GetGeneValue("c").GetValue());
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceGenomeBuilder.MakeGenomeValid"/> throws a
        /// <see cref="TimeoutException"/> if called with a genome which cannot be repaired.
        /// </summary>
        [Fact]
        public void MakeGenomeValidThrowsForImpossibleTasks()
        {
            // Create a specification in which every value is forbidden.
            var parameters = new List<IParameterNode>
                                 { new ValueNode<int>("a", new CategoricalDomain<int>(new List<int> { 7, -12 })) };

            var forbiddenValue = new Dictionary<string, IAllele> { { "a", new Allele<int>(7) } };
            var secondForbidden = new Dictionary<string, IAllele> { { "a", new Allele<int>(-12) } };
            var forbiddenCombinations = new List<ForbiddenParameterCombination>
                                            {
                                                new ForbiddenParameterCombination(forbiddenValue),
                                                new ForbiddenParameterCombination(secondForbidden),
                                            };

            var impossibleSpecification = new ParameterConfigurationSpaceSpecification(
                parameters,
                new Dictionary<string, List<EqualsCondition>>(),
                forbiddenCombinations);
            this._genomeBuilder = new ParameterConfigurationSpaceGenomeBuilder(
                AcLibUtils.CreateParameterTree(impossibleSpecification),
                impossibleSpecification,
                this._configuration);

            // Try to repair a genome.
            var genome = new Genome();
            genome.SetGene("a", new Allele<int>(7));
            Assert.Throws<TimeoutException>(() => this._genomeBuilder.MakeGenomeValid(genome));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a <see cref="ParameterConfigurationSpaceSpecification"/> with one parameter named
        /// <see cref="UnconditionalParameter"/> and one named <see cref="ConditionalParameter"/>.
        /// <see cref="ConditionalParameter"/> is only active if <see cref="UnconditionalParameter"/> is 20.
        /// <see cref="ConditionalParameter"/> may not be 0, and <see cref="UnconditionalParameter"/> may not be 59.
        /// </summary>
        /// <returns>The created <see cref="ParameterConfigurationSpaceSpecification"/>.</returns>
        private static ParameterConfigurationSpaceSpecification CreateParameterConfigurationSpaceSpecification()
        {
            var parameters = new List<IParameterNode>
                                 {
                                     new ValueNode<int>(
                                         ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter,
                                         new IntegerDomain(-30, 60)),
                                     new ValueNode<int>(ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter, new IntegerDomain(-2, 4)),
                                 };

            var condition = new EqualsCondition(
                ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter,
                new IAllele[] { new Allele<int>(20) });
            var conditions = new Dictionary<string, List<EqualsCondition>>
                                 { [ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter] = new List<EqualsCondition> { condition } };

            var forbiddenValue = new Dictionary<string, IAllele>
                                     { { ParameterConfigurationSpaceGenomeBuilderTest.ConditionalParameter, new Allele<int>(0) } };
            var secondForbidden = new Dictionary<string, IAllele>
                                      { { ParameterConfigurationSpaceGenomeBuilderTest.UnconditionalParameter, new Allele<int>(59) } };
            var forbiddenCombinations = new List<ForbiddenParameterCombination>
                                            {
                                                new ForbiddenParameterCombination(forbiddenValue),
                                                new ForbiddenParameterCombination(secondForbidden),
                                            };

            return new ParameterConfigurationSpaceSpecification(parameters, conditions, forbiddenCombinations);
        }

        #endregion
    }
}