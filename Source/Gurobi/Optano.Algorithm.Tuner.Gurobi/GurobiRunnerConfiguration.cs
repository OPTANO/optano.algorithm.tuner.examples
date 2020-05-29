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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System;
    using System.IO;
    using System.Text;

    using Optano.Algorithm.Tuner.Configuration;

    /// <summary>
    /// A configuration with gurobi-specific parameters that should not be tuned.
    /// </summary>
    public class GurobiRunnerConfiguration : ConfigurationBase
    {
        #region Public properties

        /// <summary>
        /// Gets a value indicating whether this instance is master.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is master; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaster { get; private set; }

        /// <summary>
        /// Gets the maximum number of threads that Gurobi may run on.
        /// </summary>
        public int ThreadCount { get; private set; }

        /// <summary>
        /// Gets the number of different seeds to use per intance file.
        /// For each .mps file that is found in the instance folder, <see cref="NumberOfSeeds"/> independent random seeds are drawn.
        /// </summary>
        public int NumberOfSeeds { get; private set; }

        /// <summary>
        /// Gets the random number generator seed.
        /// </summary>
        /// <value>
        /// The random seed.
        /// </value>
        public int RngSeed { get; private set; }

        /// <summary>
        /// Gets the termination mip gap of Gurobi.
        /// </summary>
        /// <value>
        /// the mip gap.
        /// </value>
        public double TerminationMipGap { get; private set; }

        /// <summary>
        /// Gets the nodefile directory of Gurobi.
        /// </summary>
        /// <value>
        /// The nodefile directory.
        /// </value>
        public DirectoryInfo NodefileDirectory { get; private set; }

        /// <summary>
        /// Gets the nodefile start size of Gurobi in gigabyte.
        /// </summary>
        /// <value>
        /// The nodefile start size in gigabyte.
        /// </value>
        public double NodefileStartSizeGigabyte { get; private set; }

        /// <summary>
        /// Gets the timeout for Gurobi runs.
        /// Value is not read from parameter args, but set later from the AlgorithmTunerConfiguration's value instead.
        /// </summary>
        public TimeSpan CpuTimeout { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public override bool IsCompatible(ConfigurationBase other)
        {
            // Currently, we cannot check other configs than the AlgorithmTunerConfiguration for compatibility.
            // Furthermore, it is not certain whether the ThreadCount or others may be changed before continuing or not.
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
            var builder = new StringBuilder("GurobiRunnerConfiguration:\r\n");
            builder.AppendLine($"{nameof(GurobiRunnerConfiguration.IsMaster)}: {this.IsMaster}")
                .AppendLine($"{nameof(GurobiRunnerConfiguration.ThreadCount)}: {this.ThreadCount}")
                .AppendLine($"{nameof(GurobiRunnerConfiguration.NumberOfSeeds)}: {this.NumberOfSeeds}")
                .AppendLine($"{nameof(GurobiRunnerConfiguration.NodefileDirectory)}: {this.NodefileDirectory}")
                .AppendLine($"{nameof(GurobiRunnerConfiguration.NodefileStartSizeGigabyte)}: {this.NodefileStartSizeGigabyte}")
                .AppendLine($"{nameof(GurobiRunnerConfiguration.CpuTimeout)}: {this.CpuTimeout}")
                .AppendLine($"{nameof(GurobiRunnerConfiguration.TerminationMipGap)}: {this.TerminationMipGap}");
            return builder.ToString();
        }

        #endregion

        /// <summary>
        /// Builder for <see cref="GurobiRunnerConfiguration"/>s.
        /// </summary>
        public class GurobiRunnerConfigBuilder : IConfigBuilder<GurobiRunnerConfiguration>
        {
            #region Static Fields

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.IsMaster"/> is <c>false</c>.
            /// </summary>
            public static readonly bool IsMasterDefault = false;

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.ThreadCount"/> is 4.
            /// </summary>
            public static readonly int ThreadCountDefault = 4;

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.NumberOfSeeds"/> is 1.
            /// </summary>
            public static readonly int NumberOfSeedsDefault = 1;

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.RngSeed"/> is 42.
            /// </summary>
            public static readonly int RngSeedDefault = 42;

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.NodefileDirectory"/> is a subfolder "nodefiles" in the current working directory.
            /// </summary>
            public static readonly string NodefileDirectoryDefault = "nodefiles";

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.NodefileStartSizeGigabyte"/> is 0,5.
            /// </summary>
            public static readonly double NodefileStartSizeGigabyteDefault = 0.5;

            #endregion

            #region Fields

            /// <summary>
            /// The default value of <see cref="GurobiRunnerConfiguration.TerminationMipGap"/> is 0,01.
            /// </summary>
            public static readonly double TerminationMipGapDefault = 0.01;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.IsMaster"/>.
            /// </summary>
            private bool? _isMaster;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.ThreadCount"/>.
            /// </summary>
            private int? _threadCount;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.NumberOfSeeds"/>.
            /// </summary>
            private int? _numberOfSeeds;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.RngSeed"/>.
            /// </summary>
            private int? _rngSeed;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.NodefileDirectory"/>.
            /// </summary>
            private DirectoryInfo _nodefileDirectory;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.NodefileStartSizeGigabyte"/>.
            /// </summary>
            private double? _nodefileStartSizeGigabyte;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.TerminationMipGap"/>.
            /// </summary>
            private double? _terminationMipGap;

            /// <summary>
            /// The value to set for <see cref="GurobiRunnerConfiguration.CpuTimeout"/>.
            /// </summary>
            private TimeSpan _cpuTimeout;

            #endregion

            #region Public properties

            /// <summary>
            /// Gets a value indicating whether this instance is master.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is master; otherwise, <c>false</c>.
            /// </value>
            public bool IsMaster => this._isMaster ?? GurobiRunnerConfigBuilder.IsMasterDefault;

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.IsMaster"/>.
            /// </summary>
            /// <param name="isMaster">if set to <c>true</c> [is master].</param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetIsMaster(bool isMaster)
            {
                this._isMaster = isMaster;
                return this;
            }

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.ThreadCount"/>.
            /// </summary>
            /// <param name="threadCount">
            /// The maximal thread count.
            /// </param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetThreadCount(int threadCount)
            {
                if (threadCount < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(threadCount), $"${nameof(threadCount)} needs to be greater than 0.");
                }

                this._threadCount = threadCount;
                return this;
            }

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.NumberOfSeeds"/>.
            /// </summary>
            /// <param name="numberOfSeeds">
            /// The number of seeds.
            /// </param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetNumberOfSeeds(int numberOfSeeds)
            {
                if (numberOfSeeds < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(numberOfSeeds), $"${nameof(numberOfSeeds)} needs to be greater than 0.");
                }

                this._numberOfSeeds = numberOfSeeds;
                return this;
            }

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.RngSeed"/>.
            /// </summary>
            /// <param name="rngSeed">The random seed.</param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetRngSeed(int rngSeed)
            {
                this._rngSeed = rngSeed;
                return this;
            }

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.NodefileDirectory"/>.
            /// </summary>
            /// <param name="nodefileDirectory">The nodefile directory.</param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetNodefileDirectory(string nodefileDirectory)
            {
                var directoryInfo = new DirectoryInfo(nodefileDirectory);
                this._nodefileDirectory = directoryInfo;
                return this;
            }

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.NodefileStartSizeGigabyte"/>.
            /// </summary>
            /// <param name="nodefileStartSizeGigabyte">The nodefile start size in gigabyte.</param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetNodefileStartSizeGigabyte(double nodefileStartSizeGigabyte)
            {
                if (nodefileStartSizeGigabyte < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(nodefileStartSizeGigabyte),
                        $"${nameof(nodefileStartSizeGigabyte)} needs to be greater than or equal to 0.");
                }

                this._nodefileStartSizeGigabyte = nodefileStartSizeGigabyte;
                return this;
            }

            /// <summary>
            /// Sets <see cref="GurobiRunnerConfiguration.TerminationMipGap"/>.
            /// </summary>
            /// <param name="terminationMipGap">The mip gap.</param>
            /// <returns><see cref="GurobiRunnerConfigBuilder"/>.</returns>
            public GurobiRunnerConfigBuilder SetTerminationMipGap(double terminationMipGap)
            {
                if (terminationMipGap < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(terminationMipGap), $"${nameof(terminationMipGap)} needs to be greater than or equal to 0.");
                }
                this._terminationMipGap = terminationMipGap;
                return this;
            }

            /// <summary>
            /// Builds the configuration using the provided
            /// <see cref="T:Optano.Algorithm.Tuner.Configuration.ConfigurationBase" /> as fallback.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>The configuration.</returns>
            public GurobiRunnerConfiguration BuildWithFallback(ConfigurationBase fallback)
            {
                var cast = ConfigurationBase.CastToConfigurationType<GurobiRunnerConfiguration>(fallback);
                return this.BuildWithFallback(cast);
            }

            /// <summary>
            /// Validates the current parameter settings and builds a corresponding <see cref="GurobiRunnerConfiguration" />.
            /// </summary>
            /// <param name="cpuTimeout">The cpu timeout.</param>
            /// <returns>
            /// The configuration.
            /// </returns>
            public GurobiRunnerConfiguration Build(TimeSpan cpuTimeout)
            {
                this._cpuTimeout = cpuTimeout;

                return this.BuildWithFallback(null);
            }

            #endregion

            #region Methods

            /// <summary>
            /// Builds the configuration using a fallback configuration.
            /// </summary>
            /// <param name="fallback">The fallback. May be null. In that case, defaults are used as fallback.</param>
            /// <returns>The configuration.</returns>
            private GurobiRunnerConfiguration BuildWithFallback(GurobiRunnerConfiguration fallback)
            {
                var config = new GurobiRunnerConfiguration
                {
                    IsMaster = this._isMaster ?? fallback?.IsMaster ?? GurobiRunnerConfigBuilder.IsMasterDefault,
                    NumberOfSeeds = this._numberOfSeeds ?? fallback?.NumberOfSeeds ?? GurobiRunnerConfigBuilder.NumberOfSeedsDefault,
                    ThreadCount = this._threadCount ?? fallback?.ThreadCount ?? GurobiRunnerConfigBuilder.ThreadCountDefault,
                    RngSeed = this._rngSeed ?? fallback?.RngSeed ?? GurobiRunnerConfigBuilder.RngSeedDefault,
                    NodefileDirectory = this._nodefileDirectory ?? fallback?.NodefileDirectory
                                                         ?? new DirectoryInfo(GurobiRunnerConfigBuilder.NodefileDirectoryDefault),
                    NodefileStartSizeGigabyte = this._nodefileStartSizeGigabyte ??
                                                                 fallback?.NodefileStartSizeGigabyte ??
                                                                 GurobiRunnerConfigBuilder.NodefileStartSizeGigabyteDefault,
                    TerminationMipGap = this._terminationMipGap ??
                                                                 fallback?.TerminationMipGap ??
                                                                 GurobiRunnerConfigBuilder.TerminationMipGapDefault,
                    CpuTimeout = this._cpuTimeout,
                };
                return config;
            }

            #endregion
        }
    }
}
