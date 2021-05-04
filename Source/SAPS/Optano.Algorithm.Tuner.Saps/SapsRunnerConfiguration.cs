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

namespace Optano.Algorithm.Tuner.Saps
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Optano.Algorithm.Tuner.Configuration;

    /// <summary>
    /// Wraps the configuration to tune SAPS.
    /// </summary>
    /// <seealso cref="Optano.Algorithm.Tuner.Configuration.ConfigurationBase" />
    public class SapsRunnerConfiguration : ConfigurationBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="SapsRunnerConfiguration"/> class from being created outside the scope of this class.
        /// </summary>
        private SapsRunnerConfiguration()
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the path to executable.
        /// </summary>
        /// <value>
        /// The path to executable.
        /// </value>
        public string PathToExecutable { get; private set; }

        /// <summary>
        /// Gets the generic parameterization.
        /// </summary>
        /// <value>
        /// The generic parameterization.
        /// </value>
        public GenericParameterization GenericParameterization { get; private set; }

        /// <summary>
        /// Gets the factor k  for the penalization of the average runtime.
        /// </summary>
        /// <value>
        /// The factor k.
        /// </value>
        public int FactorParK { get; private set; }

        /// <summary>
        /// Gets the random number generator seed.
        /// </summary>
        /// <value>
        /// The random seed.
        /// </value>
        public int RngSeed { get; private set; }

        /// <summary>
        /// Gets the number of seeds, which is used for every instance of the SAPS algorithm.
        /// </summary>
        /// <value>
        /// The number of seeds.
        /// </value>
        public int NumberOfSeeds { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public override bool IsCompatible(ConfigurationBase other)
        {
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
            var builder = new StringBuilder("SapsRunnerConfiguration:\r\n");
            builder.AppendLine($@"{nameof(SapsRunnerConfiguration.PathToExecutable)}: {this.PathToExecutable}")
                .AppendLine($"{nameof(SapsRunnerConfiguration.GenericParameterization)}: {this.GenericParameterization}")
                .AppendLine($"{nameof(SapsRunnerConfiguration.FactorParK)}: {this.FactorParK}")
                .AppendLine($"{nameof(SapsRunnerConfiguration.RngSeed)}: {this.RngSeed}")
                .AppendLine($"{nameof(SapsRunnerConfiguration.NumberOfSeeds)}: {this.NumberOfSeeds}");
            return builder.ToString();
        }

        #endregion

        /// <summary>
        /// Builds the configuration to tune SAPS.
        /// </summary>
        [SuppressMessage(
            "NDepend",
            "ND1305:NestedTypesShouldNotBeVisible",
            Justification = "This type is part of the Builder pattern.")]
        public class SapsConfigBuilder : IConfigBuilder<SapsRunnerConfiguration>
        {
            #region Static Fields

            /// <summary>
            /// The default value of <see cref="GenericParameterization"/> is the default GenericParameterziation.
            /// </summary>
            public static readonly GenericParameterization GenericParameterizationDefault = GenericParameterization.Default;

            /// <summary>
            /// The default value of <see cref="SapsRunnerConfiguration.FactorParK"/> is 0.
            /// </summary>
            public static readonly int FactorParKDefault = 0;

            /// <summary>
            /// The default value of <see cref="SapsRunnerConfiguration.RngSeed"/> is 42.
            /// </summary>
            public static readonly int RngSeedDefault = 42;

            /// <summary>
            /// The default value of <see cref="SapsRunnerConfiguration.NumberOfSeeds"/> is 1.
            /// </summary>
            public static readonly int NumberOfSeedsDefault = 1;

            #endregion

            #region Fields

            /// <summary>
            /// The value to set for <see cref="SapsRunnerConfiguration.PathToExecutable"/>.
            /// </summary>
            private string _pathToExecutable;

            /// <summary>
            /// The value to set for <see cref="GenericParameterization"/>.
            /// </summary>
            private GenericParameterization? _genericParameterization;

            /// <summary>
            /// The value to set for <see cref="SapsRunnerConfiguration.FactorParK"/>.
            /// </summary>
            private int? _factorParK;

            /// <summary>
            /// The value to set for <see cref="SapsRunnerConfiguration.RngSeed"/>.
            /// </summary>
            private int? _rngSeed;

            /// <summary>
            /// The value to set for <see cref="SapsRunnerConfiguration.NumberOfSeeds"/>.
            /// </summary>
            private int? _numberOfSeeds;

            #endregion

            #region Public properties

            /// <summary>
            /// Gets a value indicating whether this instance has path to executable.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has path to executable; otherwise, <c>false</c>.
            /// </value>
            public bool HasPathToExecutable => !string.IsNullOrWhiteSpace(this._pathToExecutable);

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Sets <see cref="SapsRunnerConfiguration.PathToExecutable"/>.
            /// </summary>
            /// <param name="pathToExecutable">The path to executable.</param>
            /// <returns><see cref="SapsConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentNullException">pathToExecutable.</exception>
            public SapsConfigBuilder SetPathToExecutable(string pathToExecutable)
            {
                this._pathToExecutable = pathToExecutable ?? throw new ArgumentNullException(nameof(pathToExecutable));
                return this;
            }

            /// <summary>
            /// Sets <see cref="SapsRunnerConfiguration.GenericParameterization"/>.
            /// </summary>
            /// <param name="genericParameterization">The generic parameterization.</param>
            /// <returns><see cref="SapsConfigBuilder"/>.</returns>
            public SapsConfigBuilder SetGenericParameterization(GenericParameterization genericParameterization)
            {
                this._genericParameterization = genericParameterization;
                return this;
            }

            /// <summary>
            /// Sets <see cref="SapsRunnerConfiguration.FactorParK"/>.
            /// </summary>
            /// <param name="factorParK">The factor k.</param>
            /// <returns><see cref="SapsConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">factorParK - factorParK.</exception>
            public SapsConfigBuilder SetFactorParK(int factorParK)
            {
                if (factorParK < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(factorParK), $"{nameof(factorParK)} needs to be greater or equal to 0.");
                }

                this._factorParK = factorParK;
                return this;
            }

            /// <summary>
            /// Sets <see cref="SapsRunnerConfiguration.RngSeed"/>.
            /// </summary>
            /// <param name="rngSeed">The random seed.</param>
            /// <returns><see cref="SapsConfigBuilder"/>.</returns>
            public SapsConfigBuilder SetRngSeed(int rngSeed)
            {
                this._rngSeed = rngSeed;
                return this;
            }

            /// <summary>
            /// Sets <see cref="SapsRunnerConfiguration.NumberOfSeeds"/>.
            /// </summary>
            /// <param name="numberOfSeeds">The number of seeds.</param>
            /// <returns><see cref="SapsConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">numberOfSeeds - numberOfSeeds.</exception>
            public SapsConfigBuilder SetNumberOfSeeds(int numberOfSeeds)
            {
                if (numberOfSeeds < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(numberOfSeeds),
                        $"{nameof(numberOfSeeds)} needs to be greater than 0.");
                }

                this._numberOfSeeds = numberOfSeeds;
                return this;
            }

            /// <summary>
            /// Builds the configuration using the provided
            /// <see cref="T:Optano.Algorithm.Tuner.Configuration.ConfigurationBase" /> as fallback.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>
            /// The configuration.
            /// </returns>
            public SapsRunnerConfiguration BuildWithFallback(ConfigurationBase fallback)
            {
                var cast = ConfigurationBase.CastToConfigurationType<SapsRunnerConfiguration>(fallback);
                return this.BuildWithFallback(cast);
            }

            /// <summary>
            /// Builds the specified configuration.
            /// </summary>
            /// <param name="pathToExecutable">The path to executable.</param>
            /// <returns>The configuration.</returns>
            public SapsRunnerConfiguration Build(string pathToExecutable)
            {
                this.SetPathToExecutable(pathToExecutable);
                return this.Build();
            }

            /// <summary>
            /// Builds the specified configuration.
            /// </summary>
            /// <returns>The configuration.</returns>
            public SapsRunnerConfiguration Build()
            {
                return this.BuildWithFallback(null);
            }

            #endregion

            #region Methods

            /// <summary>
            /// Builds the configuration using a fallback configuration.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>The configuration.</returns>
            private SapsRunnerConfiguration BuildWithFallback(SapsRunnerConfiguration fallback)
            {
                var config = new SapsRunnerConfiguration
                                 {
                                     PathToExecutable =
                                         this._pathToExecutable ?? fallback?.PathToExecutable
                                         ?? throw new InvalidOperationException("You must set the path to the executable."),
                                     GenericParameterization = this._genericParameterization ??
                                                               fallback?.GenericParameterization ?? SapsConfigBuilder.GenericParameterizationDefault,
                                     FactorParK = this._factorParK ?? fallback?.FactorParK ?? SapsConfigBuilder.FactorParKDefault,
                                     RngSeed = this._rngSeed ?? fallback?.RngSeed ?? SapsConfigBuilder.RngSeedDefault,
                                     NumberOfSeeds = this._numberOfSeeds ?? fallback?.NumberOfSeeds ?? SapsConfigBuilder.NumberOfSeedsDefault,
                                 };
                return config;
            }

            #endregion
        }
    }
}