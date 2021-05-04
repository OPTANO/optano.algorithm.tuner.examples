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

namespace Optano.Algorithm.Tuner.AcLib.Tests
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Smoke test for the adapter.
    /// </summary>
    [Collection("NonParallel")]
    [SuppressMessage(
        "NDepend",
        "ND1205:AStatelessClassOrStructureMightBeTurnedIntoAStaticType",
        Justification = "This is a test class.")]
    public class AdapterSmokeTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// Start adapter. Make sure that some time was spent during the tuning.
        /// </summary>
        [SkippableFact]
        public void SmokeTest()
        {
            // Skip, if no python 2.7 binary was resolved.
            Skip.IfNot(TestUtils.TryToResolvePython27BinaryFromPath(out var pythonBinary));

            // Update path to python 2.7 binary in scenario file.
            const string PathToScenarioFile = @"Tools\scenario.txt";
            const string PythonBinaryPlaceholder = "[PlaceholderForPathToPythonBinary]";
            var content = File.ReadAllText(PathToScenarioFile);
            content = content.Replace(PythonBinaryPlaceholder, pythonBinary.FullName);
            File.WriteAllText(PathToScenarioFile, content);

            // Start adapter.
            var timer = Stopwatch.StartNew();
            var args = new[]
                           {
                               "--master", $"--scenario={PathToScenarioFile}", "--numGens=2", "--goalGen=0", "--popSize=8",
                               "--maxParallelEvaluations=1",
                               "--miniTournamentSize=4", "--instanceNumbers=1:1",
                           };
            Program.Main(args);
            timer.Stop();
            timer.Elapsed.TotalMilliseconds.ShouldBeGreaterThan(2000);
        }

        #endregion
    }
}