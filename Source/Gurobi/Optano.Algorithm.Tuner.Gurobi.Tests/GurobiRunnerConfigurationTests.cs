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

namespace Optano.Algorithm.Tuner.Gurobi.Tests
{
    using System;
    using System.IO;
    using System.Linq;

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
        /// Checks that trying to use the builder throws an
        /// <see cref="InvalidOperationException"/> if <see cref="GurobiRunnerConfigurationParser.ParseArguments"/>
        /// was not called beforehand.
        /// </summary>
        [Fact]
        public void ConfigurationParserThrowsIfNoPreprocessingWasDone()
        {
            Exception exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                        {
                            var builder = this._parser.ConfigurationBuilder;
                        });
        }

        /// <summary>
        /// Checks that all possible arguments to an instance acting as master get parsed correctly.
        /// </summary>
        [Fact]
        public void MasterArgumentsAreParsedCorrectly()
        {
            const int ThreadCount = 8;
            const int NumberOfSeeds = 42;
            const int RngSeed = 7;
            const string NodefileDir = "dummy_directory";
            const double NodefileSize = 2;
            const double TerminationMipGap = 0.05;

            var args = new[] { "--master", $"--grbThreadCount={ThreadCount}", $"--numberOfSeeds={NumberOfSeeds}", $"--rngSeed={RngSeed}", $"--grbNodefileDirectory={NodefileDir}", $"--grbNodefileStartSizeGigabyte={NodefileSize}", $"--grbTerminationMipGap={TerminationMipGap}" };

            this._parser.ParseArguments(args);

            var config = this._parser.ConfigurationBuilder.Build(TimeSpan.FromSeconds(5));

            config.IsMaster.ShouldBeTrue("Expected master to be requested.");
            config.ThreadCount.ShouldBe(ThreadCount, "Expected different number of threads.");
            config.NumberOfSeeds.ShouldBe(NumberOfSeeds, "Expected different number of seeds.");
            config.RngSeed.ShouldBe(RngSeed, "Expected different random number generator seed.");
            config.NodefileDirectory.Name.ShouldBe(NodefileDir, "Expected different nodefile directory.");
            config.NodefileStartSizeGigabyte.ShouldBe(NodefileSize, "Expected different nodefile start size.");
            config.TerminationMipGap.ShouldBe(TerminationMipGap, "Expected different termination mip gap.");
        }

        /// <summary>
        /// Checks that all possible arguments to an instance acting as worker get parsed correctly.
        /// </summary>
        [Fact]
        public void NonMasterArgumentsAreParsedCorrectly()
        {
            const string RemainingArgs = "test";
            var args = new[] { RemainingArgs };
            this._parser.ParseArguments(args);

            var config = this._parser.ConfigurationBuilder.Build(TimeSpan.FromSeconds(5));

            config.IsMaster.ShouldBeFalse("Did not expect master to be requested.");
            this._parser.RemainingArguments.Count().ShouldBe(1, "Expected one remaining argument.");
            this._parser.RemainingArguments.First().ShouldBe(RemainingArgs, "Expected different remaining argument.");
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
            Exception exception =
                Assert.Throws<ArgumentOutOfRangeException>(
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
            Exception exception =
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
            Exception exception =
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
            Exception exception =
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

        /// <summary>
        /// Checks that <see cref="GurobiRunnerConfigurationParser.PrintHelp"/> prints help about general worker arguments, general
        /// master arguments, and custom Gurobi arguments.
        /// </summary>
        [Fact]
        public void PrintHelpPrintsAllArguments()
        {
            TestUtils.CheckOutput(
                action: () => this._parser.PrintHelp(),
                check: consoleOutput =>
                    {
                        var reader = new StringReader(consoleOutput.ToString());
                        var text = reader.ReadToEnd();
                        text.ShouldContain("Arguments for the application:", "Application arguments are missing.");
                        text.ShouldContain("Arguments for master:", "General master arguments are missing.");
                        text.ShouldContain("Arguments for worker:", "General worker arguments are missing.");
                    });
        }

        #endregion
    }
}
