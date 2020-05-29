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

namespace Optano.Algorithm.Tuner.Bbob.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using NDesk.Options;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="BbobRunnerConfiguration"/> class and the <see cref="BbobRunnerConfigurationParser"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class BbobConfigurationTests : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="BbobRunnerConfigurationParser"/> used in tests.
        /// </summary>
        private BbobRunnerConfigurationParser _parser;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BbobConfigurationTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public BbobConfigurationTests()
        {
            this._parser = new BbobRunnerConfigurationParser();
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
        /// <see cref="InvalidOperationException"/> if no preprocessing was done.
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
            const int InstanceSeed = 42;
            const string PythonBin = "dummy binary";
            const string BbobScript = "dummy script";
            const int FunctionId = 6;
            const int Dimensions = 7;
            const GenericParameterization GenericParameterization = GenericParameterization.RandomForestAverageRank;

            var args = new[]
                           {
                               "--master", $"--instanceSeed={InstanceSeed}", $"--pythonBin={PythonBin}", $"--bbobScript={BbobScript}",
                               $"--functionId={FunctionId}", $"--dimensions={Dimensions}", $"--genericParameterization={GenericParameterization}",
                           };

            this._parser.ParseArguments(args);

            var config = this._parser.ConfigurationBuilder.Build();

            config.IsMaster.ShouldBeTrue("Expected master to be requested.");
            config.InstanceSeed.ShouldBe(InstanceSeed, "Expected different random seed.");
            config.PythonBin.ShouldBe(PythonBin, "Expected different path to python binary.");
            config.PathToExecutable.ShouldBe(BbobScript, "Expected different path to BBOB python script.");
            config.FunctionId.ShouldBe(FunctionId, "Expected different function id.");
            config.Dimensions.ShouldBe(Dimensions, "Expected different number of dimensions.");
            config.GenericParameterization.ShouldBe(GenericParameterization, "Expected different generic parameterization.");
        }

        /// <summary>
        /// Checks that all possible arguments to an instance acting as worker get parsed correctly.
        /// </summary>
        [Fact]
        public void NonMasterArgumentsAreParsedCorrectly()
        {
            const string AdditionalArgs = "test";
            var args = new[] { AdditionalArgs };
            this._parser.ParseArguments(args);

            var config = this._parser.ConfigurationBuilder.Build();

            config.IsMaster.ShouldBeFalse("Did not expect master to be requested.");
            this._parser.AdditionalArguments.Count().ShouldBe(1, "Expected one additional argument.");
            this._parser.AdditionalArguments.First().ShouldBe(AdditionalArgs, "Expected different additional argument.");
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
            const string PythonBin = "dummy binary";
            const int FunctionId = 6;

            var args = new[]
                           {
                               "--master", $"--pythonBin={PythonBin}", $"--functionId={FunctionId}",
                               $"--genericParameterization={genericParameterizationString}",
                           };

            this._parser.ParseArguments(args);

            var config = this._parser.ConfigurationBuilder.Build();

            config.IsMaster.ShouldBeTrue("Expected master to be requested.");
            config.PythonBin.ShouldBe(PythonBin, "Expected different path to python binary.");
            config.FunctionId.ShouldBe(FunctionId, "Expected different function id.");
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
            const string PythonBin = "dummy binary";
            const int FunctionId = 6;

            var args = new[]
                           {
                               "--master",
                               $"--pythonBin={PythonBin}",
                               $"--functionId={FunctionId}",
                               $"--genericParameterization={genericParameterizationString}",
                           };
            Exception exception =
                Assert.Throws<OptionException>(
                    () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that providing the --master argument and a function id, but no argument defining the python binary
        /// results in an <see cref="OptionException"/> when calling
        /// <see cref="BbobRunnerConfigurationParser.ParseArguments"/>.
        /// </summary>
        [Fact]
        public void MissingPythonBinThrowsException()
        {
            var args = new[] { "--master", "--functionId=6" };
            Exception exception =
                Assert.Throws<OptionException>(
                    () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that providing the --master argument and a python binary, but no argument defining the function id
        /// results in an <see cref="OptionException"/> when calling
        /// <see cref="BbobRunnerConfigurationParser.ParseArguments"/>.
        /// </summary>
        [Fact]
        public void MissingFunctionIdThrowsException()
        {
            var args = new[] { "--master", "--pythonBin=dummy binary" };
            Exception exception =
                Assert.Throws<OptionException>(
                    () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the functionId is out of range.
        /// </summary>
        /// <param name="functionId">The function identifier.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(57)]
        public void ParserThrowsIfFunctionIdIsOutOfRange(int functionId)
        {
            const string PythonBin = "dummy binary";
            var args = new[] { "--master", $"--pyhonBin={PythonBin}", $"--functionId={functionId}" };
            Exception exception =
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that the parser throws an <see cref="ArgumentOutOfRangeException" />, if the dimensions is negative or zero.
        /// </summary>
        /// <param name="dimensions">The dimensions.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ParserThrowsIfDimensionsIsNegativeOrZero(int dimensions)
        {
            const string PythonBin = "dummy binary";
            const int FunctionId = 6;
            var args = new[] { "--master", $"--pyhonBin={PythonBin}", $"--functionId={FunctionId}", $"--dimensions={dimensions}" };
            Exception exception =
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => this._parser.ParseArguments(args));
        }

        /// <summary>
        /// Checks that <see cref="BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.BuildWithFallback"/> method prioritizes the fallback arguments correctly.
        /// </summary>
        [Fact]
        public void BuildWithFallBackPrioritizesFallbackArgumentsCorrectly()
        {
            const int InstanceSeedFallback = 500;
            const int DimensionsFallback = 300;

            var fallback = new BbobRunnerConfiguration.BbobRunnerConfigurationBuilder()
                .SetInstanceSeed(InstanceSeedFallback)
                .SetDimensions(DimensionsFallback)
                .Build();

            var config = new BbobRunnerConfiguration.BbobRunnerConfigurationBuilder()
                .SetDimensions(BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.DimensionsDefault)
                .BuildWithFallback(fallback);

            config.Dimensions.ShouldBe(BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.DimensionsDefault);
            config.InstanceSeed.ShouldBe(InstanceSeedFallback);
        }

        /// <summary>
        /// Checks that <see cref="BbobRunnerConfigurationParser.PrintHelp"/> prints help about general worker arguments, general
        /// master arguments, and custom BBOB arguments.
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
