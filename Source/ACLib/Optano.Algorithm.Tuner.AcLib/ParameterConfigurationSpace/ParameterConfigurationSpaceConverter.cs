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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    /// <summary>
    /// Responsible for converting files in Parameter Configuration Space (PCS) format
    /// (http://www.cs.ubc.ca/labs/beta/Projects/SMAC/v2.08.00/manual.pdf#page=12) to
    /// <see cref="ParameterConfigurationSpaceSpecification"/>s.
    /// </summary>
    public static class ParameterConfigurationSpaceConverter
    {
        #region Constants

        /// <summary>
        /// Regular expression for parameter names.
        /// </summary>
        private const string ParameterNameRegex = @"[^,\s""()|=]+";

        /// <summary>
        /// Regular expression for any parameter value.
        /// </summary>
        private const string ValueRegex = @"[^,\s]+";

        /// <summary>
        /// Regular expression for numbers.
        /// </summary>
        private const string NumberRegex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";

        /// <summary>
        /// Name for a named group containing a parameter name.
        /// </summary>
        private const string ParameterGroupName = "parameter_name";

        /// <summary>
        /// Name for a named group containing parameter values.
        /// </summary>
        private const string ValueGroupName = "values";

        /// <summary>
        /// Name for named group containing a numerical parameter's minimum value.
        /// </summary>
        private const string MinValueGroupName = "min_value";

        /// <summary>
        /// Name for named group containing a numerical parameter's maximum value.
        /// </summary>
        private const string MaxValueGroupName = "max_value";

        /// <summary>
        /// Name for named group which existence indicates that a numerical parameter should be discrete.
        /// </summary>
        private const string IntegerGroupName = "integer";

        /// <summary>
        /// Name for named group which existence indicates that a numerical parameter's distribution should be
        /// logarithmic.
        /// </summary>
        private const string LogScaleGroupName = "log_scale";

        /// <summary>
        /// Name for alternative group which existence indicates that a numerical parameter's distribution should be
        /// logarithmic.
        /// </summary>
        private const string AlternativeLogScaleGroupName = "alternative_log_scale";

        /// <summary>
        /// Name for named group containing the child's parameter name in
        /// <see cref="ConditionalParameterClausePattern"/>.
        /// </summary>
        private const string ChildGroupName = "child_name";

        /// <summary>
        /// Name for named group containing the parents's parameter name in
        /// <see cref="ConditionalParameterClausePattern"/>.
        /// </summary>
        private const string ParentGroupName = "parent_name";

        /// <summary>
        /// Name for named group containing a forbidden parameter combination in
        /// <see cref="ForbiddenParameterClausePattern"/>.
        /// </summary>
        private const string CombinationGroupName = "combination";

        #endregion

        #region Static Fields

        /// <summary>
        /// The regular expression for a categorical parameter clause.
        /// <para>parameter_name {value 1, ..., value N} [default value].</para>
        /// </summary>
        private static readonly Regex CategoricalParameterClausePattern = new Regex(
            $@"^(?<{ParameterConfigurationSpaceConverter.ParameterGroupName}>{ParameterConfigurationSpaceConverter.ParameterNameRegex})\s*{{(?<{ParameterConfigurationSpaceConverter.ValueGroupName}>(?:\s*{ParameterConfigurationSpaceConverter.ValueRegex}\s*,)*\s*{ParameterConfigurationSpaceConverter.ValueRegex})\s*}}");

        /// <summary>
        /// The regular expression for a numerical parameter clause.
        /// <para>parameter_name [min value, max value] [default value] [i] [l].</para>
        /// </summary>
        private static readonly Regex NumericalParameterClausePattern = new Regex(
            $@"^(?<{ParameterConfigurationSpaceConverter.ParameterGroupName}>{ParameterConfigurationSpaceConverter.ParameterNameRegex})\s*\[\s*(?<{ParameterConfigurationSpaceConverter.MinValueGroupName}>{ParameterConfigurationSpaceConverter.NumberRegex})\s*,\s*(?<{ParameterConfigurationSpaceConverter.MaxValueGroupName}>{ParameterConfigurationSpaceConverter.NumberRegex})\s*\]\s*\[.*\]\s*(?<{ParameterConfigurationSpaceConverter.AlternativeLogScaleGroupName}>l)?\s*(?<{ParameterConfigurationSpaceConverter.IntegerGroupName}>i)?\s*(?<{ParameterConfigurationSpaceConverter.LogScaleGroupName}>l)?");

        /// <summary>
        /// The regular expression for a conditional parameter clause.
        /// <para>child name | parent name in {parent val1, ..., parent valK}.</para>
        /// </summary>
        private static readonly Regex ConditionalParameterClausePattern = new Regex(
            $@"^(?<{ParameterConfigurationSpaceConverter.ChildGroupName}>{ParameterConfigurationSpaceConverter.ParameterNameRegex})\s*\|\s*(?<{ParameterConfigurationSpaceConverter.ParentGroupName}>{ParameterConfigurationSpaceConverter.ParameterNameRegex})\s*in\s*{{\s*(?<{ParameterConfigurationSpaceConverter.ValueGroupName}>(?:{ParameterConfigurationSpaceConverter.ValueRegex}\s*,\s*)*{ParameterConfigurationSpaceConverter.ValueRegex})\s*}}");

        /// <summary>
        /// The regular expression for a forbidden parameter clause.
        /// <para>{parameter name 1=value 1, ..., parameter name N=value N}.</para>
        /// </summary>
        private static readonly Regex ForbiddenParameterClausePattern = new Regex(
            $@"^{{(?<{ParameterConfigurationSpaceConverter.CombinationGroupName}>(?:\s*{ParameterConfigurationSpaceConverter.ParameterNameRegex}\s*=\s*{ParameterConfigurationSpaceConverter.ValueRegex}\s*,)*\s*{ParameterConfigurationSpaceConverter.ParameterNameRegex}\s*=\s*{ParameterConfigurationSpaceConverter.ValueRegex}\s*)}}");

        /// <summary>
        /// Regular expression for a single assignment as found in <see cref="ForbiddenParameterClausePattern"/>.
        /// <para>parameter name=value.</para>
        /// </summary>
        private static readonly Regex AssignmentPattern = new Regex(
            $@"(?<{ParameterConfigurationSpaceConverter.ParameterGroupName}>{ParameterConfigurationSpaceConverter.ParameterNameRegex})\s*=\s*(?<{ParameterConfigurationSpaceConverter.ValueGroupName}>{ParameterConfigurationSpaceConverter.ValueRegex})");

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Converts a PCS file into a <see cref="ParameterConfigurationSpaceSpecification"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>The created <see cref="ParameterConfigurationSpaceSpecification"/>.</returns>
        public static ParameterConfigurationSpaceSpecification Convert(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"No file at {path}.");
            }

            var parameters = new List<IParameterNode>();
            var conditionalParameterClauses = new List<Match>();
            var forbiddenParameterClauses = new List<Match>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                {
                    // Comment or blank line.
                    continue;
                }

                var match = ParameterConfigurationSpaceConverter.ConditionalParameterClausePattern.Match(line);
                if (match.Success)
                {
                    // Delay parsing of conditional clauses until all parameters are known.
                    conditionalParameterClauses.Add(match);
                    continue;
                }

                match = ParameterConfigurationSpaceConverter.ForbiddenParameterClausePattern.Match(line);
                if (match.Success)
                {
                    // Delay parsing of forbidden parameter clauses until all parameters are known.
                    forbiddenParameterClauses.Add(match);
                    continue;
                }

                match = ParameterConfigurationSpaceConverter.CategoricalParameterClausePattern.Match(line);
                if (match.Success)
                {
                    parameters.Add(ParameterConfigurationSpaceConverter.CreateCategoricalParameter(match));
                    continue;
                }

                match = ParameterConfigurationSpaceConverter.NumericalParameterClausePattern.Match(line);
                if (match.Success)
                {
                    parameters.Add(ParameterConfigurationSpaceConverter.CreateNumericalParameter(match));
                    continue;
                }

                throw new FormatException($"Cannot parse '{line}'. Is the format correct?");
            }

            return new ParameterConfigurationSpaceSpecification(
                parameters,
                ParameterConfigurationSpaceConverter.CreateParameterActivityConditions(conditionalParameterClauses, parameters),
                ParameterConfigurationSpaceConverter.CreateForbiddenParameterCombinations(forbiddenParameterClauses, parameters));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a categorical parameter.
        /// </summary>
        /// <param name="categoricalParameterClause">
        /// A match from <see cref="CategoricalParameterClausePattern"/>.
        /// </param>
        /// <returns>The created <see cref="IParameterNode"/>.</returns>
        private static IParameterNode CreateCategoricalParameter(Match categoricalParameterClause)
        {
            var identifier = categoricalParameterClause.Groups[ParameterConfigurationSpaceConverter.ParameterGroupName].Value;
            var values = categoricalParameterClause.Groups[ParameterConfigurationSpaceConverter.ValueGroupName].Value.Split(',')
                .Select(value => value.Trim());

            return new ValueNode<string>(identifier, new CategoricalDomain<string>(values.ToList()));
        }

        /// <summary>
        /// Creates a numerical parameter.
        /// </summary>
        /// <param name="numericalParameterClause">A match from <see cref="NumericalParameterClausePattern"/>.</param>
        /// <returns>The created <see cref="IParameterNode"/>.</returns>
        private static IParameterNode CreateNumericalParameter(Match numericalParameterClause)
        {
            var identifier = numericalParameterClause.Groups[ParameterConfigurationSpaceConverter.ParameterGroupName].Value;
            var minValue = numericalParameterClause.Groups[ParameterConfigurationSpaceConverter.MinValueGroupName].Value;
            var maxValue = numericalParameterClause.Groups[ParameterConfigurationSpaceConverter.MaxValueGroupName].Value;
            var isLogarithmic = numericalParameterClause.Groups[ParameterConfigurationSpaceConverter.LogScaleGroupName].Success
                                || numericalParameterClause.Groups[ParameterConfigurationSpaceConverter.AlternativeLogScaleGroupName].Success;

            if (numericalParameterClause.Groups[ParameterConfigurationSpaceConverter.IntegerGroupName].Success)
            {
                return ParameterConfigurationSpaceConverter.CreateIntegerParameter(identifier, minValue, maxValue, isLogarithmic);
            }
            else
            {
                return ParameterConfigurationSpaceConverter.CreateContinuousParameter(identifier, minValue, maxValue, isLogarithmic);
            }
        }

        /// <summary>
        /// Creates an integer parameter.
        /// </summary>
        /// <param name="identifier">The parameter identifier.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="isLogarithmic">Whether the parameter's distribution should be logarithmic.</param>
        /// <returns>The created <see cref="IParameterNode"/>.</returns>
        private static IParameterNode CreateIntegerParameter(
            string identifier,
            string minValue,
            string maxValue,
            bool isLogarithmic)
        {
            if (!int.TryParse(minValue, out var minimum))
            {
                throw new FormatException(
                    $"Minimum value '{minValue}' of parameter '{identifier}' should be an integer.");
            }

            if (!int.TryParse(maxValue, out var maximum))
            {
                throw new FormatException(
                    $"Maximum value '{maxValue}' of parameter '{identifier}' should be an integer.");
            }

            if (isLogarithmic)
            {
                return new ValueNode<int>(identifier, new DiscreteLogDomain(minimum, maximum));
            }
            else
            {
                return new ValueNode<int>(identifier, new IntegerDomain(minimum, maximum));
            }
        }

        /// <summary>
        /// Creates a continuous parameter.
        /// </summary>
        /// <param name="identifier">The parameter identifier.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="isLogarithmic">Whether the parameter's distribution should be logarithmic.</param>
        /// <returns>The created <see cref="IParameterNode"/>.</returns>
        private static IParameterNode CreateContinuousParameter(string identifier, string minValue, string maxValue, bool isLogarithmic)
        {
            if (!double.TryParse(minValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var minimum))
            {
                throw new FormatException(
                    $"Minimum value '{minValue}' of parameter '{identifier}' should be a continuous value.");
            }

            if (!double.TryParse(maxValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var maximum))
            {
                throw new FormatException(
                    $"Maximum value '{minValue}' of parameter '{identifier}' should be a continuous value.");
            }

            if (isLogarithmic)
            {
                return new ValueNode<double>(identifier, new LogDomain(minimum, maximum));
            }
            else
            {
                return new ValueNode<double>(identifier, new ContinuousDomain(minimum, maximum));
            }
        }

        /// <summary>
        /// Creates a mapping from parameter identifiers to conditions determining whether they are active.
        /// </summary>
        /// <param name="conditionalParameterClauses">
        /// Matches from <see cref="ConditionalParameterClausePattern"/>.
        /// </param>
        /// <param name="parameterDefinitions">All parameter definitions.</param>
        /// <returns>The created mapping.</returns>
        private static Dictionary<string, List<EqualsCondition>> CreateParameterActivityConditions(
            IEnumerable<Match> conditionalParameterClauses,
            List<IParameterNode> parameterDefinitions)
        {
            var parameterActivityConditions = new Dictionary<string, List<EqualsCondition>>();
            foreach (var clause in conditionalParameterClauses)
            {
                var childIdentifier = clause.Groups[ParameterConfigurationSpaceConverter.ChildGroupName].Value;
                if (!parameterDefinitions.Any(definition => Equals(definition.Identifier, childIdentifier)))
                {
                    throw new InvalidOperationException(
                        $"Tried to create a condition for '{childIdentifier}', which was not defined.");
                }

                if (!parameterActivityConditions.TryGetValue(childIdentifier, out var conditions))
                {
                    conditions = new List<EqualsCondition>();
                    parameterActivityConditions.Add(childIdentifier, conditions);
                }

                conditions.Add(ParameterConfigurationSpaceConverter.CreateCondition(clause, parameterDefinitions));
            }

            return parameterActivityConditions;
        }

        /// <summary>
        /// Creates forbidden parameter combinations.
        /// </summary>
        /// <param name="forbiddenParameterClauses">
        /// Matches from <see cref="ForbiddenParameterClausePattern"/>.
        /// </param>
        /// <param name="parameterDefinitions">All parameter definitions.</param>
        /// <returns>The created <see cref="ForbiddenParameterCombination"/>s.</returns>
        private static IEnumerable<ForbiddenParameterCombination> CreateForbiddenParameterCombinations(
            IEnumerable<Match> forbiddenParameterClauses,
            List<IParameterNode> parameterDefinitions)
        {
            return forbiddenParameterClauses
                .Select(clause => ParameterConfigurationSpaceConverter.CreateForbiddenParameterClause(clause, parameterDefinitions));
        }

        /// <summary>
        /// Creates a <see cref="EqualsCondition"/>.
        /// </summary>
        /// <param name="conditionalParameterClause">
        /// A match from <see cref="ConditionalParameterClausePattern"/>.
        /// </param>
        /// <param name="parameterDefinitions">All parameter definitions.</param>
        /// <returns>The created <see cref="EqualsCondition"/>.</returns>
        private static EqualsCondition CreateCondition(
            Match conditionalParameterClause,
            List<IParameterNode> parameterDefinitions)
        {
            var parentIdentifier = conditionalParameterClause.Groups[ParameterConfigurationSpaceConverter.ParentGroupName].Value;
            var domain = ParameterConfigurationSpaceConverter.FindParameterDomain(parameterDefinitions, parentIdentifier);

            var valueSpecifications = conditionalParameterClause.Groups[ParameterConfigurationSpaceConverter.ValueGroupName].Value
                .Split(',')
                .Select(value => value.Trim());

            return new EqualsCondition(
                parentIdentifier,
                valueSpecifications.Select(value => ParameterConfigurationSpaceConverter.TransformValueToAllele(domain, value)));
        }

        /// <summary>
        /// Creates a <see cref="ForbiddenParameterCombination"/>.
        /// </summary>
        /// <param name="forbiddenParameterClause">
        /// A match from <see cref="ForbiddenParameterClausePattern"/>.
        /// </param>
        /// <param name="parameterDefinitions">All parameter definitions.</param>
        /// <returns>The created <see cref="ForbiddenParameterCombination"/>.</returns>
        private static ForbiddenParameterCombination CreateForbiddenParameterClause(
            Match forbiddenParameterClause,
            List<IParameterNode> parameterDefinitions)
        {
            var completeSpecification = forbiddenParameterClause.Groups[ParameterConfigurationSpaceConverter.CombinationGroupName].Value;
            var assignmentMatches = ParameterConfigurationSpaceConverter.AssignmentPattern.Matches(completeSpecification);

            var forbiddenCombination = new Dictionary<string, IAllele>();
            foreach (Match match in assignmentMatches)
            {
                var identifier = match.Groups[ParameterConfigurationSpaceConverter.ParameterGroupName].Value;
                var value = match.Groups[ParameterConfigurationSpaceConverter.ValueGroupName].Value;
                var domain = ParameterConfigurationSpaceConverter.FindParameterDomain(parameterDefinitions, identifier);
                forbiddenCombination[identifier] = ParameterConfigurationSpaceConverter.TransformValueToAllele(domain, value);
            }

            return new ForbiddenParameterCombination(forbiddenCombination);
        }

        /// <summary>
        /// Finds a <see cref="IParameterNode"/>'s domain.
        /// </summary>
        /// <param name="parameterDefinitions">All parameter definitions.</param>
        /// <param name="parentIdentifier">Identifier of parameter to find domain for.</param>
        /// <returns>The parameter's <see cref="IDomain"/>.</returns>
        private static IDomain FindParameterDomain(
            IEnumerable<IParameterNode> parameterDefinitions,
            string parentIdentifier)
        {
            var matchingDefinitions =
                parameterDefinitions.Where(node => Equals(node.Identifier, parentIdentifier));
            if (matchingDefinitions.Count() != 1)
            {
                throw new InvalidOperationException(
                    $"Tried to create a clause dependent on '{parentIdentifier}', which was either defined multiple times or not at all.");
            }

            return matchingDefinitions.Single().Domain;
        }

        /// <summary>
        /// Transforms the string-typed value to an <see cref="IAllele"/>.
        /// </summary>
        /// <param name="domain">The domain the <see cref="IAllele"/> should be in.</param>
        /// <param name="value">The value to transform.</param>
        /// <returns>The created <see cref="IAllele"/>.</returns>
        private static IAllele TransformValueToAllele(IDomain domain, string value)
        {
            if (domain.IsCategoricalDomain)
            {
                // We have only created string categorical domains.
                return new Allele<string>(value);
            }

            switch (domain)
            {
                case DiscreteLogDomain _:
                case IntegerDomain _:
                    if (!int.TryParse(value, out var integerValue))
                    {
                        throw new FormatException(
                            $"One of the clauses specifies a value of '{value}' for a discrete parameter.");
                    }

                    return new Allele<int>(integerValue);
                case LogDomain _:
                case ContinuousDomain _:
                    if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var continuousValue))
                    {
                        throw new FormatException(
                            $"One of the clauses specifies a value of '{value}' for a continuous parameter.");
                    }

                    return new Allele<double>(continuousValue);
                default:
                    throw new NotSupportedException(
                        $"Domain with type {domain.GetType()} is not supported by conditional parameter clauses and should never have been created.");
            }
        }

        #endregion
    }
}