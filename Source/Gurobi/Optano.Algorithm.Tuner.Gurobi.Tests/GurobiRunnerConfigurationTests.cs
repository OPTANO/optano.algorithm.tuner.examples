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

namespace Optano.Algorithm.Tuner.Gurobi.Tests
{
    using System;
    using System.Linq;

    using NDesk.Options;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="GurobiRunnerConfiguration"/> class and the <see cref="GurobiRunnerConfigurationParser"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiRunnerConfigurationTests : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="GurobiRunnerConfigurationParser"/> used in tests.
        /// </summary>
        private GurobiRunnerConfigurationParser _parser;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunnerConfigurationTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiRunnerConfigurationTests()
        {
            this._parser = new GurobiRunnerConfigurationParser();
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
            this._parser = null;
        }

        /// <summary>
        /// Checks that all arguments get parsed correctly.
        /// </summary>
        /// <param name="runMode">The run mode.</param>
        [Theory]
        [InlineData("master")]
        [InlineData("postTuning")]
        public void ArgumentsGetParsedCorrectly(string runMode)
        {
            const GurobiTertiaryTuneCriterion TertiaryTuneCriterion = GurobiTertiaryTuneCriterion.None;
            const int ThreadCount = 8;
            const string NodefileDir = "dummy_directory";
            const double NodefileSize = 2;
            const double TerminationMipGap = 0.05;
            const int NumberOfSeeds = 42;
            const int RngSeed = 7;

            var args = new[]
                           {
                               $"--{runMode}",
                               $"--grbThreadCount={ThreadCount}",
                               $"--grbNodefileDirectory={NodefileDir}",
                               $"--grbNodefileStartSizeGigabyte={NodefileSize}",
                               $"--grbTerminationMipGap={TerminationMipGap}",
                               $"--tertiaryTuneCriterion={TertiaryTuneCriterion}",
                               $"--numberOfSeeds={NumberOfSeeds}",
                               $"--rngSeed={RngSeed}",
                           };

            this._parser.ParseArguments(args);
            var config = this._parser.ConfigurationBuilder.Build(TimeSpan.FromSeconds(5));

            config.ThreadCount.ShouldBe(ThreadCount);
            config.NodefileDirectory.Name.ShouldBe(NodefileDir);
            config.NodefileStartSizeGigabyte.ShouldBe(NodefileSize);
            config.TerminationMipGap.ShouldBe(TerminationMipGap);

            switch (runMode)
            {
                case "master":
                    this._parser.IsMaster.ShouldBeTrue();
                    config.TertiaryTuneCriterion.ShouldBe(TertiaryTuneCriterion);
                    config.NumberOfSeeds.ShouldBe(NumberOfSeeds);
                    config.RngSeed.ShouldBe(RngSeed);
                    break;
                case "postTuning":
                    this._parser.IsPostTuningRunner.ShouldBeTrue();
                    this._parser.AdditionalArguments.Count().ShouldBe(3);
                    this._parser.AdditionalArguments.First().ShouldBe($"--tertiaryTuneCriterion={TertiaryTuneCriterion}");
                    this._parser.AdditionalArguments.Skip(1).First().ShouldBe($"--numberOfSeeds={NumberOfSeeds}");
                    this._parser.AdditionalArguments.Skip(2).First().ShouldBe($"--rngSeed={RngSeed}");
                    break;
                default:
                    Assert.True(false);
                    break;
            }
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the number of threads is negative or zero.
        /// </summary>
        /// <param name="threadCount">The thread count.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ParserThrowsIfThreadCountIsNegativeOrZero(int threadCount)
        {
            var args = new[] { "--master", $"--grbThreadCount={threadCount}" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="OptionException" />, if the tertiary tune criterion is not a member of the <see cref="GurobiTertiaryTuneCriterion"/> enum.
        /// </summary>
        [Fact]
        public void ParserThrowsIfTertiaryTuneCriterionIsNotMemberOfEnum()
        {
            var args = new[] { "--master", "--tertiaryTuneCriterion=dummy" };
            Assert.Throws<OptionException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the number of seeds is negative or zero.
        /// </summary>
        /// <param name="numberOfSeeds">The number of seeds.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ParserThrowsIfNumberOfSeedsIsNegativeOrZero(int numberOfSeeds)
        {
            var args = new[] { "--master", $"--numberOfSeeds={numberOfSeeds}" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the nodefile start size is negative.
        /// </summary>
        /// <param name="nodefileSize">Size of the nodefile.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(-2.5)]
        public void ParserThrowsIfNodeFileStartSizeGigabyteIsNegative(double nodefileSize)
        {
            var args = new[] { "--master", $"--grbNodefileStartSizeGigabyte={nodefileSize}" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the termination mip gap is negative.
        /// </summary>
        /// <param name="terminationMipGap">The mip gap.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(-2.5)]
        public void ParserThrowsIfTerminationMipGapIsNegative(double terminationMipGap)
        {
            var args = new[] { "--master", $"--grbTerminationMipGap={terminationMipGap}" };
            Assert.Throws<ArgumentOutOfRangeException>(
                () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that <see cref="GurobiRunnerConfiguration.GurobiRunnerConfigBuilder.BuildWithFallback"/> method prioritizes the fallback arguments correctly.
        /// </summary>
        [Fact]
        public void BuildWithFallBackPrioritizesFallbackArgumentsCorrectly()
        {
            const int ThreadCountFallback = 12;
            const int NumberOfSeedsFallback = 10;

            var fallback = new GurobiRunnerConfiguration.GurobiRunnerConfigBuilder()
                .SetThreadCount(ThreadCountFallback)
                .SetNumberOfSeeds(NumberOfSeedsFallback)
                .Build(TimeSpan.FromSeconds(5));

            var config = new GurobiRunnerConfiguration.GurobiRunnerConfigBuilder()
                .SetThreadCount(GurobiRunnerConfiguration.GurobiRunnerConfigBuilder.ThreadCountDefault)
                .BuildWithFallback(fallback);

            config.ThreadCount.ShouldBe(GurobiRunnerConfiguration.GurobiRunnerConfigBuilder.ThreadCountDefault);
            config.NumberOfSeeds.ShouldBe(NumberOfSeedsFallback);
        }

        #endregion
    }
}