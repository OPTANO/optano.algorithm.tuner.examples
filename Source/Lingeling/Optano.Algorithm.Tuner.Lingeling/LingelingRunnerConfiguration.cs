#region Copyright (c) OPTANO GmbH

// ////////////////////////////////////////////////////////////////////////////////
// 
//        OPTANO GmbH Source Code
//        Copyright (c) 2010-2020 OPTANO GmbH
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

namespace Optano.Algorithm.Tuner.Lingeling
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Optano.Algorithm.Tuner.Configuration;

    /// <summary>
    /// Wraps the configuration to tune Lingeling.
    /// </summary>
    /// <seealso cref="ConfigurationBase" />
    public class LingelingRunnerConfiguration : ConfigurationBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="LingelingRunnerConfiguration"/> class from being created outside the scope of this class.
        /// </summary>
        private LingelingRunnerConfiguration()
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets a value indicating whether this instance is master.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is master; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaster { get; private set; }

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
        /// Gets the number of seeds, which is used for every instance of the Lingeling algorithm.
        /// </summary>
        /// <value>
        /// The number of seeds.
        /// </value>
        public int NumberOfSeeds { get; private set; }

        /// <summary>
        /// Gets the memory limit in megabyte, which should be used for the algorithm.
        /// </summary>
        /// <value>
        /// The memory limit in megabyte.
        /// </value>
        public int MemoryLimitMegabyte { get; private set; }

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
            var builder = new StringBuilder("LingelingRunnerConfiguration:\r\n");
            builder.AppendLine($"{nameof(this.IsMaster)}: {this.IsMaster}")
                .AppendLine($@"{nameof(this.PathToExecutable)}: {this.PathToExecutable}")
                .AppendLine($"{nameof(this.GenericParameterization)}: {this.GenericParameterization}")
                .AppendLine($"{nameof(this.FactorParK)}: {this.FactorParK}")
                .AppendLine($"{nameof(this.RngSeed)}: {this.RngSeed}")
                .AppendLine($"{nameof(this.NumberOfSeeds)}: {this.NumberOfSeeds}")
                .AppendLine($"{nameof(this.MemoryLimitMegabyte)}: {this.MemoryLimitMegabyte}");

            return builder.ToString();
        }

        #endregion

        /// <summary>
        /// Builds the configuration to tune Lingeling.
        /// </summary>
        [SuppressMessage(
            "NDepend",
            "ND1305:NestedTypesShouldNotBeVisible",
            Justification = "This type is part of the Builder pattern.")]
        public class LingelingConfigBuilder : IConfigBuilder<LingelingRunnerConfiguration>
        {
            #region Static Fields

            /// <summary>
            /// The default value of <see cref="LingelingRunnerConfiguration.IsMaster"/> is <c>false</c>.
            /// </summary>
            public static readonly bool IsMasterDefault = false;

            /// <summary>
            /// The default value of <see cref="PathToExecutable"/> is an empty string.
            /// </summary>
            public static readonly string PathToExecutableDefault = string.Empty;

            /// <summary>
            /// The default value of <see cref="GenericParameterization"/> is the default GenericParameterziation.
            /// </summary>
            public static readonly GenericParameterization GenericParameterizationDefault =
                GenericParameterization.Default;

            /// <summary>
            /// The default value of <see cref="FactorParK"/> is 10.
            /// </summary>
            public static readonly int FactorParKDefault = 10;

            /// <summary>
            /// The default value of <see cref="RngSeed"/> is 42.
            /// </summary>
            public static readonly int RngSeedDefault = 42;

            /// <summary>
            /// The default value of <see cref="NumberOfSeeds"/> is 1.
            /// </summary>
            public static readonly int NumberOfSeedsDefault = 1;

            /// <summary>
            /// The default value of <see cref="MemoryLimitMegabyte"/> is 4000.
            /// </summary>
            public static readonly int MemoryLimitMegabyteDefault = 4000;

            #endregion

            #region Fields

            /// <summary>
            /// The value to set for <see cref="LingelingRunnerConfiguration.IsMaster"/>.
            /// </summary>
            private bool? _isMaster;

            /// <summary>
            /// The value to set for <see cref="PathToExecutable"/>.
            /// </summary>
            private string _pathToExecutable;

            /// <summary>
            /// The value to set for <see cref="GenericParameterization"/>.
            /// </summary>
            private GenericParameterization? _genericParameterization;

            /// <summary>
            /// The value to set for <see cref="FactorParK"/>.
            /// </summary>
            private int? _factorParK;

            /// <summary>
            /// The value to set for <see cref="RngSeed"/>.
            /// </summary>
            private int? _rngSeed;

            /// <summary>
            /// The value to set for <see cref="NumberOfSeeds"/>.
            /// </summary>
            private int? _numberOfSeeds;

            /// <summary>
            /// The value to set for <see cref="MemoryLimitMegabyte"/>.
            /// </summary>
            private int? _memoryLimitMegabyte;

            #endregion

            #region Public properties

            /// <summary>
            /// Gets a value indicating whether this instance is master.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is master; otherwise, <c>false</c>.
            /// </value>
            public bool IsMaster => this._isMaster ?? LingelingConfigBuilder.IsMasterDefault;

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
            /// Sets the boolean <see cref="LingelingRunnerConfiguration.IsMaster"/>.
            /// </summary>
            /// <param name="isMaster">The value to set the boolean to.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            public LingelingConfigBuilder SetIsMaster(bool isMaster)
            {
                this._isMaster = isMaster;
                return this;
            }

            /// <summary>
            /// Sets <see cref="PathToExecutable"/>.
            /// </summary>
            /// <param name="pathToExecutable">The path to executable.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentNullException">pathToExecutable.</exception>
            public LingelingConfigBuilder SetPathToExecutable(string pathToExecutable)
            {
                this._pathToExecutable = pathToExecutable ?? throw new ArgumentNullException(nameof(pathToExecutable));
                return this;
            }

            /// <summary>
            /// Sets <see cref="GenericParameterization"/>.
            /// </summary>
            /// <param name="genericParameterization">The generic parameterization.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            public LingelingConfigBuilder SetGenericParameterization(GenericParameterization genericParameterization)
            {
                this._genericParameterization = genericParameterization;
                return this;
            }

            /// <summary>
            /// Sets <see cref="FactorParK"/>.
            /// </summary>
            /// <param name="factorParK">The factor k.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">factorParK.</exception>
            public LingelingConfigBuilder SetFactorParK(int factorParK)
            {
                if (factorParK < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(factorParK),
                        $"{nameof(factorParK)} needs to be strictly greater than 0.");
                }

                this._factorParK = factorParK;
                return this;
            }

            /// <summary>
            /// Sets <see cref="RngSeed"/>.
            /// </summary>
            /// <param name="rngSeed">The random seed.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            public LingelingConfigBuilder SetRngSeed(int rngSeed)
            {
                this._rngSeed = rngSeed;
                return this;
            }

            /// <summary>
            /// Sets <see cref="NumberOfSeeds"/>.
            /// </summary>
            /// <param name="numberOfSeeds">The number of seeds.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">numberOfSeeds.</exception>
            public LingelingConfigBuilder SetNumberOfSeeds(int numberOfSeeds)
            {
                if (numberOfSeeds < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(numberOfSeeds),
                        $"{nameof(numberOfSeeds)} needs to be strictly greater than 0.");
                }

                this._numberOfSeeds = numberOfSeeds;
                return this;
            }

            /// <summary>
            /// Sets <see cref="MemoryLimitMegabyte"/>.
            /// </summary>
            /// <param name="memoryLimitMegabyte">The memory limit on megabyte.</param>
            /// <returns><see cref="LingelingConfigBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">memoryLimitMegabyte.</exception>
            public LingelingConfigBuilder SetMemoryLimitMegabyte(int memoryLimitMegabyte)
            {
                if (memoryLimitMegabyte < 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(memoryLimitMegabyte),
                        $"{nameof(memoryLimitMegabyte)} needs to be strictly greater than 0.");
                }

                this._memoryLimitMegabyte = memoryLimitMegabyte;
                return this;
            }

            /// <summary>
            /// Builds the configuration using the provided
            /// <see cref="T:Optano.Algorithm.Tuner.Configuration.ConfigurationBase" /> as fallback.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>
            /// The configuration.</returns>
            public LingelingRunnerConfiguration BuildWithFallback(ConfigurationBase fallback)
            {
                var cast = ConfigurationBase.CastToConfigurationType<LingelingRunnerConfiguration>(fallback);
                return this.BuildWithFallback(cast);
            }

            /// <summary>
            /// Builds the specified configuration.
            /// </summary>
            /// <param name="pathToExecutable">The path to executable.</param>
            /// <returns>The configuration.</returns>
            public LingelingRunnerConfiguration Build(string pathToExecutable)
            {
                this.SetPathToExecutable(pathToExecutable);
                return this.Build();
            }

            /// <summary>
            /// Builds the specified configuration.
            /// </summary>
            /// <returns>The configuration.</returns>
            public LingelingRunnerConfiguration Build()
            {
                return this.BuildWithFallback(null);
            }

            #endregion

            #region Methods

            /// <summary>
            /// Builds the configuration using a fallback configuration.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>
            /// The configuration.
            /// </returns>
            private LingelingRunnerConfiguration BuildWithFallback(LingelingRunnerConfiguration fallback)
            {
                var config = new LingelingRunnerConfiguration
                                 {
                                     IsMaster = this._isMaster ?? fallback?.IsMaster ?? LingelingConfigBuilder.IsMasterDefault,
                                     PathToExecutable = this._pathToExecutable
                                                        ?? fallback?.PathToExecutable ?? LingelingConfigBuilder.PathToExecutableDefault,
                                     GenericParameterization = this._genericParameterization ??
                                                               fallback?.GenericParameterization
                                                               ?? LingelingConfigBuilder.GenericParameterizationDefault,
                                     FactorParK = this._factorParK ?? fallback?.FactorParK ?? LingelingConfigBuilder.FactorParKDefault,
                                     RngSeed = this._rngSeed ?? fallback?.RngSeed ?? LingelingConfigBuilder.RngSeedDefault,
                                     NumberOfSeeds = this._numberOfSeeds ?? fallback?.NumberOfSeeds ?? LingelingConfigBuilder.NumberOfSeedsDefault,
                                     MemoryLimitMegabyte = this._memoryLimitMegabyte ??
                                                           fallback?.MemoryLimitMegabyte ?? LingelingConfigBuilder.MemoryLimitMegabyteDefault,
                                 };
                return config;
            }

            #endregion
        }
    }
}