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

namespace Optano.Algorithm.Tuner.Bbob.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="BbobRunner"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class BbobRunnerTests : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// The bbob script used in tests.
        /// </summary>
        private static readonly string BbobScript = @"Tools/bbobeval.py";

        /// <summary>
        /// The instance file path used in tests.
        /// </summary>
        private static readonly string SeedFilePath = "testInstance.bbi";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BbobRunnerTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public BbobRunnerTests()
        {
            BbobRunnerTests.DeleteInstanceFile();
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
            BbobRunnerTests.DeleteInstanceFile();
        }

        /// <summary>
        /// Smoke test for the BBOB adapter.
        /// </summary>
        [Fact]
        public void SmokeTest()
        {
            var timer = Stopwatch.StartNew();
            var args = new[]
                           {
                               "--master", "--maxParallelEvaluations=2", "--trainingInstanceFolder=Tools",
                               "--popSize=8", "--miniTournamentSize=4", "--cpuTimeout=5", "--instanceNumbers=1:1",
                               "--numGens=2", "--goalGen=0", "--pythonBin=python", "--functionId=17",
                           };
            Program.Main(args);
            timer.Stop();
            timer.Elapsed.TotalMilliseconds.ShouldBeGreaterThan(2000);
        }

        /// <summary>
        /// Checks, if the example runs get the right results.
        /// </summary>
        /// <param name="functionId">The function identifier.</param>
        /// <param name="instanceSeed">The instance seed.</param>
        /// <param name="x0">The x0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="expectedResult">The expected result.</param>
        [Theory]
        [InlineData(5, 10, 5, 10, 50, 550)]
        [InlineData(20, 20, 10, 17, 3, 789448.639488)]
        [InlineData(13, 16, 4, 19, 7, 3502.952977)]
        public void BbobRunnerReturnsCorrectResult(int functionId, int instanceSeed, double x0, double x1, double x2, double expectedResult)
        {
            var pythonBinary = TestUtils.ResolvePython27Binary();

            var parameters = new Dictionary<string, IAllele>
                                 {
                                     ["x0"] = new Allele<double>(x0),
                                     ["x1"] = new Allele<double>(x1),
                                     ["x2"] = new Allele<double>(x2),
                                 };
            var bbobRunner = new BbobRunner(functionId, parameters, pythonBinary.FullName, BbobRunnerTests.BbobScript);

            var instance = BbobRunnerTests.CreateInstanceFile(instanceSeed);

            var runnerTask = bbobRunner.Run(instance, CancellationToken.None);
            runnerTask.Wait();
            var result = runnerTask.Result;
            result.Value.ShouldBe(expectedResult, "Expected different result.");
        }

        /// <summary>
        /// Checks that <see cref="BbobRunner.ExtractFunctionValue"/> returns the correct function value.
        /// </summary>
        /// <param name="functionValue">The function value.</param>
        [Theory]
        [InlineData(7)]
        [InlineData(4.5)]
        public void ExtractFunctionValueReturnsCorrectFunctionValue(double functionValue)
        {
            var output = $"result={functionValue.ToString(CultureInfo.InvariantCulture)}";
            var extractedFunctionValue = BbobRunner.ExtractFunctionValue(output);
            extractedFunctionValue.ShouldBe(functionValue, "Expected different function value.");
        }

        /// <summary>
        /// Checks that <see cref="BbobRunner.ExtractFunctionValue"/> returns the maximal double value, if the output is corrupted.
        /// </summary>
        /// <param name="output">The output.</param>
        [Theory]
        [InlineData("corruptedResult=7")]
        [InlineData("corruptedResult=4.5")]
        [InlineData("")]
        [InlineData("=23")]
        public void ExtractFunctionValueReturnsMaxValueIfResultIsCorrupted(string output)
        {
            var extractedFunctionValue = BbobRunner.ExtractFunctionValue(output);
            extractedFunctionValue.ShouldBe(double.MaxValue, "Expected different function value.");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the instance file.
        /// </summary>
        /// <param name="instanceSeed">The instance seed.</param>
        /// <returns>The instance file.</returns>
        private static InstanceFile CreateInstanceFile(int instanceSeed)
        {
            File.WriteAllText(BbobRunnerTests.SeedFilePath, $"{instanceSeed}");
            var instance = new InstanceFile(BbobRunnerTests.SeedFilePath);
            return instance;
        }

        /// <summary>
        /// Deletes the instance file.
        /// </summary>
        private static void DeleteInstanceFile()
        {
            if (File.Exists(BbobRunnerTests.SeedFilePath))
            {
                File.Delete(BbobRunnerTests.SeedFilePath);
            }
        }

        #endregion
    }
}