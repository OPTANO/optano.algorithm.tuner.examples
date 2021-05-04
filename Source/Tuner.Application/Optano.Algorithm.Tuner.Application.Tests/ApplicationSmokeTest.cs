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

namespace Optano.Algorithm.Tuner.Application.Tests
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

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
    public class ApplicationSmokeTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// Start adapter. Make sure that some time was spent during the tuning.
        /// </summary>
        [Fact]
        public void SmokeTest()
        {
            var timer = Stopwatch.StartNew();
            var args = new[]
                           {
                               "--master",
                               "--basicCommand=Tools/ubcsat.exe -alg saps -i {instance} {arguments} -timeout 1 -cutoff max -seed 42\"",
                               "--parameterTree=Tools/sapsParameterTree.xml", "--numGens=2", "--goalGen=0", "--popSize=8",
                               "--maxParallelEvaluations=1",
                               "--trainingInstanceFolder=Tools/Instances", "--miniTournamentSize=4", "--cpuTimeout=1", "--instanceNumbers=1:1",
                           };
            Program.Main(args);
            timer.Stop();
            timer.Elapsed.TotalMilliseconds.ShouldBeGreaterThan(2000);
        }

        #endregion
    }
}