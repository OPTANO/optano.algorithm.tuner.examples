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

namespace Optano.Algorithm.Tuner.Bbob
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Optano.Algorithm.Tuner.Configuration;

    /// <summary>
    /// Wraps the configuration to tune BBOB.
    /// </summary>
    /// <seealso cref="Optano.Algorithm.Tuner.Configuration.ConfigurationBase" />
    public class BbobRunnerConfiguration : ConfigurationBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Prevents a default instance of the <see cref="BbobRunnerConfiguration"/> class from being created outside the scope of this class.
        /// </summary>
        private BbobRunnerConfiguration()
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
        /// Gets the random seed for the instance generator seed.
        /// </summary>
        /// <value>
        /// The instance seed.
        /// </value>
        public int InstanceSeed { get; private set; }

        /// <summary>
        /// Gets the python bin.
        /// </summary>
        /// <value>
        /// The python bin.
        /// </value>
        public string PythonBin { get; private set; }

        /// <summary>
        /// Gets the path to executable.
        /// </summary>
        /// <value>
        /// The path to executable.
        /// </value>
        public string PathToExecutable { get; private set; }

        /// <summary>
        /// Gets the function identifier.
        /// </summary>
        /// <value>
        /// The function identifier.
        /// </value>
        public int FunctionId { get; private set; }

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        public int Dimensions { get; private set; }

        /// <summary>
        /// Gets the generic parameterization.
        /// </summary>
        /// <value>
        /// The generic parameterization.
        /// </value>
        public GenericParameterization GenericParameterization { get; private set; }

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
            var builder = new StringBuilder("BbobRunnerConfiguration:\r\n");
            builder.AppendLine($"{nameof(BbobRunnerConfiguration.IsMaster)}: {this.IsMaster}")
                .AppendLine($"{nameof(BbobRunnerConfiguration.InstanceSeed)}: {this.InstanceSeed}")
                .AppendLine($@"{nameof(BbobRunnerConfiguration.PythonBin)}: {this.PythonBin}")
                .AppendLine($@"{nameof(BbobRunnerConfiguration.PathToExecutable)}: {this.PathToExecutable}")
                .AppendLine($"{nameof(BbobRunnerConfiguration.FunctionId)}: {this.FunctionId}")
                .AppendLine($"{nameof(BbobRunnerConfiguration.Dimensions)}: {this.Dimensions}")
                .AppendLine($"{nameof(BbobRunnerConfiguration.GenericParameterization)}: {this.GenericParameterization}");
            return builder.ToString();
        }

        #endregion

        /// <summary>
        /// Builds the configuration to tune BBOB.
        /// </summary>
        [SuppressMessage(
            "NDepend",
            "ND1305:NestedTypesShouldNotBeVisible",
            Justification = "This type is part of the Builder pattern.")]
        public class BbobRunnerConfigurationBuilder : IConfigBuilder<BbobRunnerConfiguration>
        {
            #region Static Fields

            /// <summary>
            /// The default value of <see cref="BbobRunnerConfiguration.IsMaster"/>.
            /// </summary>
            public static readonly bool IsMasterDefault = false;

            /// <summary>
            /// The default value of <see cref="BbobRunnerConfiguration.InstanceSeed"/>.
            /// </summary>
            public static readonly int InstanceSeedDefault = 42;

            /// <summary>
            /// The default value of <see cref="BbobRunnerConfiguration.PathToExecutable"/>.
            /// </summary>
            public static readonly string PathToExecutableDefault = @"Tools/bbobeval.py";

            /// <summary>
            /// The default value of <see cref="BbobRunnerConfiguration.Dimensions"/>.
            /// </summary>
            public static readonly int DimensionsDefault = 10;

            /// <summary>
            /// The default value of <see cref="BbobRunnerConfiguration.GenericParameterization"/>.
            /// </summary>
            public static readonly GenericParameterization GenericParameterizationDefault =
                GenericParameterization.Default;

            #endregion

            #region Fields

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.IsMaster"/>.
            /// </summary>
            private bool? _isMaster;

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.InstanceSeed"/>.
            /// </summary>
            private int? _instanceSeed;

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.PythonBin"/>.
            /// </summary>
            private string _pythonBin;

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.PathToExecutable"/>.
            /// </summary>
            private string _pathToExecutable;

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.FunctionId"/>.
            /// </summary>
            private int? _functionId;

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.Dimensions"/>.
            /// </summary>
            private int? _dimensions;

            /// <summary>
            /// The value to set for <see cref="BbobRunnerConfiguration.GenericParameterization"/>.
            /// </summary>
            private GenericParameterization? _genericParameterization;

            #endregion

            #region Public properties

            /// <summary>
            /// Gets a value indicating whether this instance is master.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is master; otherwise, <c>false</c>.
            /// </value>
            public bool IsMaster => this._isMaster ?? BbobRunnerConfigurationBuilder.IsMasterDefault;

            /// <summary>
            /// Gets a value indicating whether this instance has python bin.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has python bin; otherwise, <c>false</c>.
            /// </value>
            public bool HasPythonBin => !string.IsNullOrWhiteSpace(this._pythonBin);

            /// <summary>
            /// Gets a value indicating whether this instance has function id.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has function id; otherwise, <c>false</c>.
            /// </value>
            public bool HasFunctionId => this._functionId.HasValue;

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Sets the boolean <see cref="IsMaster"/>.
            /// </summary>
            /// <param name="isMaster">if set to <c>true</c> [is master].</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            public BbobRunnerConfigurationBuilder SetIsMaster(bool isMaster)
            {
                this._isMaster = isMaster;
                return this;
            }

            /// <summary>
            /// Sets the random seed for the instance generator seed.
            /// </summary>
            /// <param name="instanceSeed">The random seed for the instance generator seed.</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            public BbobRunnerConfigurationBuilder SetInstanceSeed(int instanceSeed)
            {
                this._instanceSeed = instanceSeed;
                return this;
            }

            /// <summary>
            /// Sets the python binary.
            /// </summary>
            /// <param name="pythonBin">The python binary.</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            /// <exception cref="ArgumentException">pythonBin.</exception>
            public BbobRunnerConfigurationBuilder SetPythonBin(string pythonBin)
            {
                if (string.IsNullOrWhiteSpace(pythonBin))
                {
                    throw new ArgumentException(nameof(pythonBin));
                }

                this._pythonBin = pythonBin;
                return this;
            }

            /// <summary>
            /// Sets the path to executable.
            /// </summary>
            /// <param name="pathToExecutable">The path to executable.</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            /// <exception cref="ArgumentException">pathToExecutable.</exception>
            public BbobRunnerConfigurationBuilder SetPathToExecutable(string pathToExecutable)
            {
                if (string.IsNullOrWhiteSpace(pathToExecutable))
                {
                    throw new ArgumentException(nameof(pathToExecutable));
                }

                this._pathToExecutable = pathToExecutable;
                return this;
            }

            /// <summary>
            /// Sets the BBOB function id.
            /// </summary>
            /// <param name="functionId">The BBOB function id.</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">functionId.</exception>
            public BbobRunnerConfigurationBuilder SetFunctionId(int functionId)
            {
                if (functionId < 1 || functionId > 56)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(functionId),
                        $"{nameof(functionId)} needs to be greater than 0 and smaller than 57.");
                }

                this._functionId = functionId;
                return this;
            }

            /// <summary>
            /// Sets the number of dimensions of the BBOB function.
            /// </summary>
            /// <param name="dimensions">The number of dimensions of the BBOB function.</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            /// <exception cref="ArgumentOutOfRangeException">dimensions.</exception>
            public BbobRunnerConfigurationBuilder SetDimensions(int dimensions)
            {
                if (dimensions < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(dimensions), $"{nameof(dimensions)} needs to be greater than 0.");
                }

                this._dimensions = dimensions;
                return this;
            }

            /// <summary>
            /// Sets the generic parameterization.
            /// </summary>
            /// <param name="genericParameterization">The generic parameterization.</param>
            /// <returns><see cref="BbobRunnerConfigurationBuilder"/>.</returns>
            public BbobRunnerConfigurationBuilder SetGenericParameterization(GenericParameterization genericParameterization)
            {
                this._genericParameterization = genericParameterization;
                return this;
            }

            /// <summary>
            /// Builds the configuration using the provided
            /// <see cref="T:Optano.Algorithm.Tuner.Configuration.ConfigurationBase" /> as fallback.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>
            /// The configuration./>.
            /// </returns>
            public BbobRunnerConfiguration BuildWithFallback(ConfigurationBase fallback)
            {
                var cast = ConfigurationBase.CastToConfigurationType<BbobRunnerConfiguration>(fallback);
                return this.BuildWithFallback(cast);
            }

            /// <summary>
            /// Builds the specified configuration.
            /// </summary>
            /// <param name="pythonBin">The python binary.</param>
            /// <param name="functionId">The BBOB function id.</param>
            /// <returns>The builded configuration.</returns>
            public BbobRunnerConfiguration Build(string pythonBin, int functionId)
            {
                this.SetPythonBin(pythonBin);
                this.SetFunctionId(functionId);
                return this.Build();
            }

            /// <summary>
            /// Builds the specified configuration.
            /// </summary>
            /// <returns>The builded configuration.</returns>
            public BbobRunnerConfiguration Build()
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
            /// <exception cref="InvalidOperationException">
            /// You must set the PythonBin and FunctionId.
            /// </exception>
            private BbobRunnerConfiguration BuildWithFallback(BbobRunnerConfiguration fallback)
            {
                var config = new BbobRunnerConfiguration
                                 {
                                     IsMaster = this._isMaster ?? fallback?.IsMaster ?? BbobRunnerConfigurationBuilder.IsMasterDefault,
                                     InstanceSeed =
                                         this._instanceSeed ?? fallback?.InstanceSeed ?? BbobRunnerConfigurationBuilder.InstanceSeedDefault,
                                 };
                config.PythonBin = this._pythonBin ?? fallback?.PythonBin ??
                                   (config.IsMaster
                                        ? throw new InvalidOperationException(
                                              $"You must set the {BbobRunnerConfigurationParser.PythonBinName}.")
                                        : string.Empty);
                config.PathToExecutable =
                    this._pathToExecutable ?? fallback?.PathToExecutable ?? BbobRunnerConfigurationBuilder.PathToExecutableDefault;
                config.FunctionId = this._functionId ?? fallback?.FunctionId ??
                                    (config.IsMaster
                                         ? throw new InvalidOperationException(
                                               $"You must set the {BbobRunnerConfigurationParser.FunctionIdName}.")
                                         : int.MaxValue);
                config.Dimensions = this._dimensions ?? fallback?.Dimensions ?? BbobRunnerConfigurationBuilder.DimensionsDefault;
                config.GenericParameterization = this._genericParameterization ??
                                                 fallback?.GenericParameterization ?? BbobRunnerConfigurationBuilder.GenericParameterizationDefault;
                return config;
            }

            #endregion
        }
    }
}