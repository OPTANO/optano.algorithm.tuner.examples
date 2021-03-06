﻿#region Copyright (c) OPTANO GmbH

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

namespace Optano.Algorithm.Tuner.Lingeling.Tests
{
    using System;

    using NDesk.Options;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Containts tests for the <see cref="LingelingRunnerConfiguration"/> class and the <see cref="LingelingRunnerConfigurationParser"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class LingelingRunnerConfigurationTests : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="LingelingRunnerConfigurationParser"/> used in tests.
        /// </summary>
        private LingelingRunnerConfigurationParser _parser;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LingelingRunnerConfigurationTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public LingelingRunnerConfigurationTests()
        {
            this._parser = new LingelingRunnerConfigurationParser();
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
            this._parser = null;
        }

        /// <summary>
        /// Checks that all options are parsed correctly.
        /// </summary>
        [Fact]
        public void OptionsAreParsedCorrectly()
        {
            const string Executable = "dummy exe";
            const GenericParameterization GenericParameterization = GenericParameterization.RandomForestAverageRank;
            const int Factor = 12;
            const int Seed = 22;
            const int NumberOfSeeds = 5;
            const int MemoryLimit = 2000;
            var args = new[]
                           {
                               "--master", $"--executable={Executable}", $"--genericParameterization={GenericParameterization}",
                               $"--factorParK={Factor}", $"--rngSeed={Seed}", $"--numberOfSeeds={NumberOfSeeds}",
                               $"--memoryLimitMegabyte={MemoryLimit}",
                           };

            this._parser.ParseArguments(args);
            var config = this._parser.ConfigurationBuilder.Build();

            this._parser.IsMaster.ShouldBeTrue("Expected master to be requested.");
            config.PathToExecutable.ShouldBe(Executable, "Expected different path to executable.");
            config.GenericParameterization.ShouldBe(GenericParameterization, "Expected different generic parameterization.");
            config.FactorParK.ShouldBe(Factor, "Expected different factor.");
            config.RngSeed.ShouldBe(Seed, "Expected different random number generator seed.");
            config.NumberOfSeeds.ShouldBe(NumberOfSeeds, "Expected different number of seeds.");
            config.MemoryLimitMegabyte.ShouldBe(MemoryLimit, "Expected different memory limit.");
        }

        /// <summary>
        /// Checks that <see cref="NDesk.Options" /> parses all possible generic parameterizations correctly.
        /// </summary>
        /// <param name="genericParameterization">The generic parameterization.</param>
        /// <param name="genericParameterizationString">The generic parameterization as string.</param>
        [Theory]
        [InlineData(GenericParameterization.RandomForestReuseOldTrees, "RandomForestReuseOldTrees")]
        [InlineData(GenericParameterization.RandomForestAverageRank, "RandomForestAverageRank")]
        [InlineData(GenericParameterization.StandardRandomForest, "StandardRandomForest")]
        [InlineData(GenericParameterization.Default, "Default")]
        [InlineData(GenericParameterization.RandomForestReuseOldTrees, "randomforestreuseoldtrees")]
        [InlineData(GenericParameterization.RandomForestAverageRank, "randomforestaveragerank")]
        [InlineData(GenericParameterization.StandardRandomForest, "standardrandomforest")]
        [InlineData(GenericParameterization.Default, "default")]
        public void NDeskParsesGenericParameterizationsCorrectly(
            GenericParameterization genericParameterization,
            string genericParameterizationString)
        {
            const string Executable = "dummy exe";

            var args = new[] { "--master", $"--executable={Executable}", $"--genericParameterization={genericParameterizationString}" };

            this._parser.ParseArguments(args);
            var config = this._parser.ConfigurationBuilder.Build();

            this._parser.IsMaster.ShouldBeTrue("Expected master to be requested.");
            config.PathToExecutable.ShouldBe(Executable, "Expected different path to executable.");
            config.GenericParameterization.ShouldBe(genericParameterization, "Expected different generic parameterization");
        }

        /// <summary>
        /// Checks that <see cref="NDesk.Options" /> throws an <see cref="OptionException" />, if unknown generic parameterizations are used.
        /// </summary>
        /// <param name="genericParameterizationString">The generic parameterization as string.</param>
        [Theory]
        [InlineData("ReuseOldTrees")]
        [InlineData("AverageRank")]
        [InlineData("Standard")]
        public void NDeskThrowsIfUnknownGenericParameterizationIsUsed(string genericParameterizationString)
        {
            const string Executable = "dummy exe";

            var args = new[] { "--master", $"--executable={Executable}", $"--genericParameterization={genericParameterizationString}" };

            Assert.Throws<OptionException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that providing the --master argument, but no argument defining the path to the executable
        /// results in an <see cref="OptionException"/> when parsing.
        /// </summary>
        [Fact]
        public void MissingPathToExecutableThrowsException()
        {
            var args = new[] { "--master" };
            Assert.Throws<OptionException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the factorParK is negative.
        /// </summary>
        [Fact]
        public void ParserThrowsIfFactorParKIsNegative()
        {
            const string Executable = "dummy exe";
            var args = new[] { "--master", $"--executable={Executable}", $"--factorParK=-1" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the numberOfSeeds is negative or zero.
        /// </summary>
        /// <param name="numberOfSeeds">The number of seeds.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ParserThrowsIfNumberOfSeedsIsNegativeOrZero(int numberOfSeeds)
        {
            const string Executable = "dummy exe";
            var args = new[] { "--master", $"--executable={Executable}", $"--numberOfSeeds={numberOfSeeds}" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the memoryLimit is negative or zero.
        /// </summary>
        /// <param name="memoryLimit">The memory limit.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ParserThrowsIfMemoryLimitIsNegativeOrZero(int memoryLimit)
        {
            const string Executable = "dummy exe";
            var args = new[] { "--master", $"--executable={Executable}", $"--memoryLimitMegabyte={memoryLimit}" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that <see cref="LingelingRunnerConfiguration.LingelingConfigBuilder.BuildWithFallback"/> method prioritizes the fallback arguments correctly.
        /// </summary>
        [Fact]
        public void BuildWithFallBackPrioritizesFallbackArgumentsCorrectly()
        {
            const string PathToExecutableFallback = "dummy";
            const int NumberOfSeedsFallback = 10;
            const int RngSeedFallback = 12;

            var fallback = new LingelingRunnerConfiguration.LingelingConfigBuilder()
                .SetPathToExecutable(PathToExecutableFallback)
                .SetNumberOfSeeds(NumberOfSeedsFallback)
                .SetRngSeed(RngSeedFallback)
                .Build();

            var config = new LingelingRunnerConfiguration.LingelingConfigBuilder()
                .SetRngSeed(LingelingRunnerConfiguration.LingelingConfigBuilder.RngSeedDefault)
                .BuildWithFallback(fallback);

            config.PathToExecutable.ShouldBe(PathToExecutableFallback);
            config.RngSeed.ShouldBe(LingelingRunnerConfiguration.LingelingConfigBuilder.RngSeedDefault);
            config.NumberOfSeeds.ShouldBe(NumberOfSeedsFallback);
        }

        #endregion
    }
}