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

namespace Optano.Algorithm.Tuner.AcLib.Configuration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Optano.Algorithm.Tuner.Configuration;

    /// <summary>
    /// Contains all relevant parameters for this adapter.
    /// </summary>
    public class AcLibRunnerConfiguration : ConfigurationBase
    {
        #region Public properties

        /// <summary>
        /// Gets the path to the scenario file.
        /// </summary>
        public string PathToScenarioFile { get; private set; }

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
            var builder = new StringBuilder("AcLibRunnerConfiguration:\r\n");
            builder.AppendLine($"{nameof(AcLibRunnerConfiguration.PathToScenarioFile)}: {this.PathToScenarioFile}");
            return builder.ToString();
        }

        #endregion

        /// <summary>
        /// The <see cref="IConfigBuilder{TConfiguration}"/> of <see cref="AcLibRunnerConfiguration"/>.
        /// </summary>
        [SuppressMessage(
            "NDepend",
            "ND1305:NestedTypesShouldNotBeVisible",
            Justification = "This type is part of the Builder pattern.")]
        public class AcLibRunnerConfigurationBuilder : IConfigBuilder<AcLibRunnerConfiguration>
        {
            #region Fields

            /// <summary>
            /// The path to the scenario file.
            /// </summary>
            private string _pathToScenarioFile;

            #endregion

            #region Public properties

            /// <summary>
            /// Gets a value indicating whether this instance has path to scenario file.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has path to scenario file; otherwise, <c>false</c>.
            /// </value>
            public bool HasPathToScenarioFile => !string.IsNullOrWhiteSpace(this._pathToScenarioFile);

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Sets <see cref="AcLibRunnerConfiguration.PathToScenarioFile"/>.
            /// </summary>
            /// <param name="path">The path to the scenario file.</param>
            /// <returns>The <see cref="AcLibRunnerConfigurationBuilder"/> in its new state.</returns>
            public AcLibRunnerConfigurationBuilder SetPathToScenarioFile(string path)
            {
                this._pathToScenarioFile = path;
                return this;
            }

            /// <summary>
            /// Builds the <see cref="AcLibRunnerConfiguration"/>.
            /// </summary>
            /// <returns>The <see cref="AcLibRunnerConfiguration"/>.</returns>
            public AcLibRunnerConfiguration Build()
            {
                return this.BuildWithFallback(null);
            }

            /// <summary>
            /// Builds the <see cref="AcLibRunnerConfiguration"/>.
            /// </summary>
            /// <param name="path">The path to the scenario file.</param>
            /// <returns>The <see cref="AcLibRunnerConfiguration"/>.</returns>
            public AcLibRunnerConfiguration Build(string path)
            {
                this._pathToScenarioFile = path;

                return this.BuildWithFallback(null);
            }

            /// <inheritdoc />
            public AcLibRunnerConfiguration BuildWithFallback(ConfigurationBase fallback)
            {
                return this.BuildWithFallback(ConfigurationBase.CastToConfigurationType<AcLibRunnerConfiguration>(fallback));
            }

            #endregion

            #region Methods

            /// <summary>
            /// Builds the <see cref="AcLibRunnerConfiguration"/> with fallback.
            /// </summary>
            /// <param name="fallback">The fallback.</param>
            /// <returns>The <see cref="AcLibRunnerConfiguration"/>.</returns>
            private AcLibRunnerConfiguration BuildWithFallback(AcLibRunnerConfiguration fallback)
            {
                var config = new AcLibRunnerConfiguration
                                 {
                                     PathToScenarioFile = this._pathToScenarioFile ?? fallback?.PathToScenarioFile
                                                          ?? throw new InvalidOperationException("You must set the path to the scenario file!"),
                                 };
                return config;
            }

            #endregion
        }
    }
}