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

namespace Optano.Algorithm.Tuner.AcLib.Tests.Configuration
{
    using System;
    using System.IO;

    using Optano.Algorithm.Tuner.AcLib.Configuration;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="Scenario"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ScenarioTest : IDisposable
    {
        #region Constants

        /// <summary>
        /// Path for ACLib scenario file.
        /// </summary>
        private const string ScenarioSpecificationFile = "specification.txt";

        #endregion

        #region Fields

        /// <summary>
        /// A helper object to write scenario files.
        /// </summary>
        private readonly ScenarioFileWriter _scenarioFileWriter = new ScenarioFileWriter();

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
            if (File.Exists(ScenarioTest.ScenarioSpecificationFile))
            {
                File.Delete(ScenarioTest.ScenarioSpecificationFile);
            }
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="FileNotFoundException"/> if called
        /// with a non-existing path.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForNonExistingFile()
        {
            Assert.Throws<FileNotFoundException>(() => new Scenario("does/not/exist"));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="ArgumentNullException"/> if called
        /// with a scenario file which does not specify a command.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingCommand()
        {
            this._scenarioFileWriter.Command = null;
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            Assert.Throws<ArgumentNullException>(() => new Scenario(ScenarioTest.ScenarioSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="ArgumentNullException"/> if called
        /// with a scenario file which does not specify a training instance file.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingTrainingInstanceFile()
        {
            this._scenarioFileWriter.InstanceFile = null;
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            Assert.Throws<ArgumentNullException>(() => new Scenario(ScenarioTest.ScenarioSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="ArgumentNullException"/> if called
        /// with a scenario file which does not specify a parameter file.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingParameterFile()
        {
            this._scenarioFileWriter.ParameterFile = null;
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            Assert.Throws<ArgumentNullException>(() => new Scenario(ScenarioTest.ScenarioSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="ArgumentNullException"/> if called
        /// with a scenario file which does not specify a cutoff time.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForMissingCutoffTime()
        {
            this._scenarioFileWriter.CutoffTime = null;
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            Assert.Throws<ArgumentNullException>(() => new Scenario(ScenarioTest.ScenarioSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="FormatException"/> if called
        /// with a scenario file which specifies a cutoff time that is not an integer.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForWrongCutoffTimeFormat()
        {
            this._scenarioFileWriter.CutoffTime = "12.5";
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            Assert.Throws<FormatException>(() => new Scenario(ScenarioTest.ScenarioSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/>'s constructor throws a <see cref="FormatException"/> if called
        /// with a scenario file which specifies an unknown overall objective.
        /// </summary>
        [Fact]
        public void ConstructorThrowsForWrongObjectiveFormat()
        {
            this._scenarioFileWriter.OverallObjective = "meanHAHA";
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            Assert.Throws<FormatException>(() => new Scenario(ScenarioTest.ScenarioSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/> correctly parses an ACLib scenario file specifying a quality tuning.
        /// </summary>
        [Fact]
        public void QualityTuningIsParsedCorrectly()
        {
            var qualityTuningWriter = new ScenarioFileWriter
                                          {
                                              Command = "quality tune",
                                              CutoffTime = "1400",
                                              InstanceFile = "foo/bar.txt",
                                              TestInstanceFile = "foo/test.txt",
                                              ParameterFile = "test.pcs",
                                              OverallObjective = "mean",
                                              RunObjective = "quality",
                                          };
            qualityTuningWriter.Write(ScenarioTest.ScenarioSpecificationFile);

            var scenario = new Scenario(ScenarioTest.ScenarioSpecificationFile);
            Assert.Equal("quality tune", scenario.Command);
            Assert.Equal(TimeSpan.FromSeconds(1400), scenario.CutoffTime);
            Assert.Equal("foo/bar.txt", scenario.InstanceFile);
            Assert.Equal("foo/test.txt", scenario.TestInstanceFile);
            Assert.Equal("test.pcs", scenario.ParameterFile);
            Assert.True(scenario.OptimizeQuality);
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/> correctly parses an ACLib scenario file specifying a plain runtime
        /// tuning.
        /// </summary>
        [Fact]
        public void PlainRuntimeTuningIsParsedCorrectly()
        {
            var runtimeTuningWriter = new ScenarioFileWriter
                                          {
                                              Command = "tune",
                                              CutoffTime = "1400",
                                              InstanceFile = "foo/bar.txt",
                                              TestInstanceFile = "foo/test.txt",
                                              ParameterFile = "test.pcs",
                                              OverallObjective = "mean",
                                              RunObjective = "runtime",
                                          };
            runtimeTuningWriter.Write(ScenarioTest.ScenarioSpecificationFile);

            var scenario = new Scenario(ScenarioTest.ScenarioSpecificationFile);
            Assert.Equal("tune", scenario.Command);
            Assert.Equal(TimeSpan.FromSeconds(1400), scenario.CutoffTime);
            Assert.Equal("foo/bar.txt", scenario.InstanceFile);
            Assert.Equal("foo/test.txt", scenario.TestInstanceFile);
            Assert.Equal("test.pcs", scenario.ParameterFile);
            Assert.False(scenario.OptimizeQuality);
            Assert.Equal(1, scenario.PenalizationFactor);
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/> correctly parses an ACLib scenario file specifying a PAR-K runtime
        /// tuning.
        /// </summary>
        [Fact]
        public void PenalizedRuntimeTuningIsParsedCorrectly()
        {
            var qualityTuningWriter = new ScenarioFileWriter
                                          {
                                              Command = "tune",
                                              CutoffTime = "1400",
                                              InstanceFile = "foo/bar.txt",
                                              TestInstanceFile = "foo/test.txt",
                                              ParameterFile = "test.pcs",
                                              OverallObjective = "mean23",
                                              RunObjective = "runtime",
                                          };
            qualityTuningWriter.Write(ScenarioTest.ScenarioSpecificationFile);

            var scenario = new Scenario(ScenarioTest.ScenarioSpecificationFile);
            Assert.Equal("tune", scenario.Command);
            Assert.Equal(TimeSpan.FromSeconds(1400), scenario.CutoffTime);
            Assert.Equal("foo/bar.txt", scenario.InstanceFile);
            Assert.Equal("foo/test.txt", scenario.TestInstanceFile);
            Assert.Equal("test.pcs", scenario.ParameterFile);
            Assert.False(scenario.OptimizeQuality);
            Assert.Equal(23, scenario.PenalizationFactor);
        }

        /// <summary>
        /// Checks that <see cref="Scenario"/> correctly parses an ACLib scenario file not specifying a test instance
        /// file.
        /// </summary>
        [Fact]
        public void MissingTestInstanceFileIsHandledCorrectly()
        {
            this._scenarioFileWriter.TestInstanceFile = null;
            this._scenarioFileWriter.Write(ScenarioTest.ScenarioSpecificationFile);
            var scenario = new Scenario(ScenarioTest.ScenarioSpecificationFile);

            Assert.Null(scenario.TestInstanceFile);
        }

        #endregion
    }
}