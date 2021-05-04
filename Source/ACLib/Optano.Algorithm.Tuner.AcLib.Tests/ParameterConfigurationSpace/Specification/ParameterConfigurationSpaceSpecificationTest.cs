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
    using System.Linq;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ParameterConfigurationSpaceSpecification"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ParameterConfigurationSpaceSpecificationTest : IDisposable
    {
        #region Public Methods and Operators

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceSpecification.ExtractActiveParameters"/> throws a
        /// <see cref="ArgumentNullException"/> if called with <c>null</c>.
        /// </summary>
        [Fact]
        public void ExtractActiveParametersThrowsForParametersNull()
        {
            var specification = new ParameterConfigurationSpaceSpecification(
                new List<IParameterNode> { new ValueNode<double>("a", new ContinuousDomain()) },
                new Dictionary<string, List<EqualsCondition>>(),
                new List<ForbiddenParameterCombination>());
            Assert.Throws<ArgumentNullException>(() => specification.ExtractActiveParameters(parameters: null));
        }

        /// <summary>
        /// Checks that <see cref="ParameterConfigurationSpaceSpecification.ExtractActiveParameters"/> removes inactive
        /// parameters while it copies active ones.
        /// </summary>
        [Fact]
        public void ExtractActiveParametersRemovesInactiveParameters()
        {
            var parameters = new List<IParameterNode>
                                 {
                                     new ValueNode<int>("unconditional", new IntegerDomain()),
                                     new ValueNode<double>("unconditional_2", new ContinuousDomain()),
                                     new ValueNode<int>("conditions_met", new IntegerDomain()),
                                     new ValueNode<int>("one_condition_met", new IntegerDomain()),
                                 };
            var conditionsMet = new List<EqualsCondition>
                                    {
                                        new EqualsCondition("unconditional", new IAllele[] { new Allele<int>(214) }),
                                        new EqualsCondition("unconditional_2", new IAllele[] { new Allele<double>(-4) }),
                                    };
            var oneConditionMet = new List<EqualsCondition>
                                      {
                                          new EqualsCondition("unconditional", new IAllele[] { new Allele<int>(214) }),
                                          new EqualsCondition("unconditional_2", new IAllele[] { new Allele<double>(-8) }),
                                      };
            var specification = new ParameterConfigurationSpaceSpecification(
                parameters,
                new Dictionary<string, List<EqualsCondition>> { { "conditions_met", conditionsMet }, { "one_condition_met", oneConditionMet } },
                new List<ForbiddenParameterCombination>());

            var parameterValues = new Dictionary<string, IAllele>
                                      {
                                          ["unconditional"] = new Allele<int>(214),
                                          ["unconditional_2"] = new Allele<double>(-4),
                                          ["conditions_met"] = new Allele<int>(24),
                                          ["one_condition_met"] = new Allele<int>(23),
                                      };
            var activeParameters = specification.ExtractActiveParameters(parameterValues);

            Assert.Equal(
                3,
                activeParameters.Count);
            foreach (var expectedParameter in parameterValues.Where(p => !Equals("one_condition_met", p.Key)))
            {
                if (!activeParameters.TryGetValue(expectedParameter.Key, out var activeValue))
                {
                    Assert.True(false, $"{expectedParameter.Key} should be active.");
                }

                Assert.Equal(
                    expectedParameter.Value,
                    activeValue);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion
    }
}