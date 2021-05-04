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

namespace Optano.Algorithm.Tuner.Application
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Optano.Algorithm.Tuner.Configuration;

    /// <summary>
    /// Contains all relevant parameters for this adapter.
    /// </summary>
    public class ApplicationRunnerConfiguration : ConfigurationBase
    {
        #region Public properties

        /// <summary>
        /// Gets the basic command to the target algorithm.
        /// </summary>
        public string BasicCommand { get; private set; }

        /// <summary>
        /// Gets the path to an XML file defining the parameter tree.
        /// </summary>
        public string PathToParameterTree { get; private set; }

        /// <summary>
        /// Gets a value indicating whether whether the target algorithm should be tuned by value (otherwise, it's tuned by runtime).
        /// </summary>
        public bool TuneByValue { get; private set; }

        /// <summary>
        /// Gets a value indicating whether whether lower values are better than higher ones.
        /// </summary>
        public bool SortValuesAscendingly { get; private set; }

        /// <summary>
        /// Gets the factor for the penalization of the average runtime.
        /// </summary>
        public int FactorParK { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public override bool IsCompatible(ConfigurationBase other)
        {
            // Currently, we cannot check other configs than the AlgorithmTunerConfiguration for compatibility.
            return true;
        }

        /// <inheritdoc />
        public override bool IsTechnicallyCompatible(ConfigurationBase other)
        {
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder("ApplicationRunnerConfiguration:\r\n");
            builder.AppendLine($"{nameof(ApplicationRunnerConfiguration.BasicCommand)}: {this.BasicCommand}")
                .AppendLine($"{nameof(ApplicationRunnerConfiguration.PathToParameterTree)}: {this.PathToParameterTree}")
                .AppendLine($"{nameof(ApplicationRunnerConfiguration.TuneByValue)}: {this.TuneByValue}")
                .AppendLine($"{nameof(ApplicationRunnerConfiguration.SortValuesAscendingly)}: {this.SortValuesAscendingly}")
                .AppendLine($"{nameof(ApplicationRunnerConfiguration.FactorParK)}: {this.FactorParK}");
            return builder.ToString();
        }

        #endregion

        /// <summary>
        /// The <see cref="IConfigBuilder{TConfiguration}"/> of <see cref="ApplicationRunnerConfiguration"/>.
        /// </summary>
        [SuppressMessage(
            "NDepend",
            "ND1305:NestedTypesShouldNotBeVisible",
            Justification = "This type is part of the Builder pattern.")]
        public class ApplicationConfigurationBuilder : IConfigBuilder<ConfigurationBase>
        {
            #region Fields

            /// <summary>
            /// The basic command to the target algorithm as specified by the parsed arguments.
            /// </summary>
            private string _basicCommand;

            /// <summary>
            /// The path to an XML file defining the parameter tree as specified by the parsed arguments.
            /// </summary>
            private string _pathToParameterTree;

            /// <summary>
            /// A value indicating whether the target algorithm should be tuned by value (otherwise, it's tuned by runtime).
            /// </summary>
            private bool? _tuneByValue = false;

            /// <summary>
            /// A value indicating whether lower values are better than higher ones.
            /// </summary>
            private bool? _sortValuesAscendingly = true;

            /// <summary>
            /// The factor for the penalization of the average runtime.
            /// </summary>
            private int? _factorParK = 0;

            #endregion

            #region Public properties

            /// <summary>
            /// Gets a value indicating whether this instance has path to parameter tree.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has path to parameter tree; otherwise, <c>false</c>.
            /// </value>
            public bool HasPathToParameterTree => !string.IsNullOrWhiteSpace(this._pathToParameterTree);

            /// <summary>
            /// Gets a value indicating whether this instance has basic command.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has basic command; otherwise, <c>false</c>.
            /// </value>
            public bool HasBasicCommand => !string.IsNullOrWhiteSpace(this._basicCommand);

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Sets <see cref="ApplicationRunnerConfiguration.BasicCommand"/>.
            /// </summary>
            /// <param name="basicCommand">The basic command.</param>
            /// <returns>The <see cref="ApplicationConfigurationBuilder"/> in its new state.</returns>
            public ApplicationConfigurationBuilder SetBasicCommand(string basicCommand)
            {
                this._basicCommand = basicCommand;
                return this;
            }

            /// <summary>
            /// Sets <see cref="ApplicationRunnerConfiguration.PathToParameterTree"/>.
            /// </summary>
            /// <param name="pathToParameterTree">The path to the parameter tree.</param>
            /// <returns>The <see cref="ApplicationConfigurationBuilder"/> in its new state.</returns>
            public ApplicationConfigurationBuilder SetPathToParameterTree(string pathToParameterTree)
            {
                this._pathToParameterTree = pathToParameterTree;
                return this;
            }

            /// <summary>
            /// Sets <see cref="ApplicationRunnerConfiguration.TuneByValue"/>.
            /// </summary>
            /// <param name="tuneByValue">The TuneByValue-Boolean.</param>
            /// <returns>The <see cref="ApplicationConfigurationBuilder"/> in its new state.</returns>
            public ApplicationConfigurationBuilder SetTuneByValue(bool tuneByValue)
            {
                this._tuneByValue = tuneByValue;
                return this;
            }

            /// <summary>
            /// Sets <see cref="ApplicationRunnerConfiguration.SortValuesAscendingly"/>.
            /// </summary>
            /// <param name="sortValuesAscendingly">The SortValuesAscendingly-Boolean.</param>
            /// <returns>The <see cref="ApplicationConfigurationBuilder"/> in its new state.</returns>
            public ApplicationConfigurationBuilder SetSortValuesAscendingly(bool sortValuesAscendingly)
            {
                this._sortValuesAscendingly = sortValuesAscendingly;
                return this;
            }

            /// <summary>
            /// Sets <see cref="ApplicationRunnerConfiguration.FactorParK"/>.
            /// </summary>
            /// <param name="factorParK">The factor for the penalization of the average runtime.</param>
            /// <returns>The <see cref="ApplicationConfigurationBuilder"/> in its new state.</returns>
            public ApplicationConfigurationBuilder SetFactorParK(int factorParK)
            {
                if (factorParK < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(factorParK),
                        $"{nameof(factorParK)} needs to be greater or equal to 0.");
                }

                this._factorParK = factorParK;
                return this;
            }

            /// <summary>
            /// Builds the <see cref="ApplicationRunnerConfiguration"/>.
            /// </summary>
            /// <returns>
            /// The <see cref="ApplicationRunnerConfiguration"/>.
            /// </returns>
            public ApplicationRunnerConfiguration Build()
            {
                return this.BuildWithFallback(null);
            }

            /// <summary>
            /// Builds the <see cref="ApplicationRunnerConfiguration"/>.
            /// </summary>
            /// <param name="basicCommand">The basic command.</param>
            /// <param name="pathToParameterTree">The path to the parameter tree.</param>
            /// <returns>
            /// The <see cref="ApplicationRunnerConfiguration"/>.
            /// </returns>
            public ApplicationRunnerConfiguration Build(string basicCommand, string pathToParameterTree)
            {
                this._basicCommand = basicCommand;
                this._pathToParameterTree = pathToParameterTree;

                return this.BuildWithFallback(null);
            }

            /// <inheritdoc />
            public ConfigurationBase BuildWithFallback(ConfigurationBase fallback)
            {
                return this.BuildWithFallback(ConfigurationBase.CastToConfigurationType<ApplicationRunnerConfiguration>(fallback));
            }

            #endregion

            #region Methods

            /// <summary>
            /// Builds the <see cref="ApplicationRunnerConfiguration"/> with fallback.
            /// </summary>
            /// <param name="fallback">The fallback.</param>
            /// <returns>
            /// The <see cref="ApplicationRunnerConfiguration"/>.
            /// </returns>
            private ApplicationRunnerConfiguration BuildWithFallback(ApplicationRunnerConfiguration fallback)
            {
                var config = new ApplicationRunnerConfiguration
                                 {
                                     BasicCommand = this._basicCommand ?? fallback?.BasicCommand
                                                    ?? throw new InvalidOperationException("You must set the basic command!"),
                                     PathToParameterTree = this._pathToParameterTree ?? fallback?.PathToParameterTree
                                                           ?? throw new InvalidOperationException("You must set the path to the parameter tree!"),
                                     TuneByValue = this._tuneByValue ?? fallback?.TuneByValue ?? false,
                                     SortValuesAscendingly = this._sortValuesAscendingly ?? fallback?.SortValuesAscendingly
                                                             ?? true,
                                     FactorParK = this._factorParK ??
                                                  fallback?.FactorParK ??
                                                  0,
                                 };
                return config;
            }

            #endregion
        }
    }
}