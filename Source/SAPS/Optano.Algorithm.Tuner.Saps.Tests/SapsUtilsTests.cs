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

namespace Optano.Algorithm.Tuner.Saps.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="SapsUtils"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class SapsUtilsTests : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// File names that should be translated into instances on <see cref="SapsUtils.CreateInstances"/>.
        /// </summary>
        private static readonly string[] CnfFileNames = { "useful1.cnf", "useful2.cnf" };

        /// <summary>
        /// File names that should not be translated into instances on <see cref="SapsUtils.CreateInstances"/>.
        /// </summary>
        private static readonly string[] NonCnfFileNames = { "useless.txt" };

        #endregion

        #region Fields

        /// <summary>
        /// Path to the folder containing test data. Has to be initialized.
        /// </summary>
        private readonly string _instanceFolder;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SapsUtilsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public SapsUtilsTests()
        {
            this._instanceFolder = PathUtils.GetAbsolutePathFromExecutableFolderRelative("testData");
            Directory.CreateDirectory(this._instanceFolder);
            foreach (var fileName in SapsUtilsTests.CnfFileNames.Union(SapsUtilsTests.NonCnfFileNames))
            {
                var handle = File.Create(Path.Combine(this._instanceFolder, fileName));
                handle.Close();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
            Directory.Delete(this._instanceFolder, recursive: true);
        }

        /// <summary>
        /// Checks that the <see cref="ParameterTree"/> returned by <see cref="SapsUtils.CreateParameterTree()"/>
        /// contains 4 independent parameters:
        /// * alpha, a logarithmically spaced parameter between 1.01 and 1.4;
        /// * rho, a uniformly spaced parameter between 0 and 1;
        /// * ps, a uniformly spaced parameter between 0 and 0.2; and
        /// * wp, a uniformly spaced parameter between 0 and 0.06.
        /// </summary>
        [Fact]
        public void CreateParameterTreeReturnsIndependentSapsParameters()
        {
            var tree = SapsUtils.CreateParameterTree();

            // Check root is an and node, i.e. independent parameters follow.
            var root = tree.Root;
            (root is AndNode).ShouldBeTrue("Parameters are not independent.");

            // Try and get all parameters needed for Saps from those node's children.
            var alpha = root.Children.Single(child => TestUtils.RepresentsParameter(child, "alpha")) as IParameterNode;
            var rho = root.Children.Single(child => TestUtils.RepresentsParameter(child, "rho")) as IParameterNode;
            var ps = root.Children.Single(child => TestUtils.RepresentsParameter(child, "ps")) as IParameterNode;
            var wp = root.Children.Single(child => TestUtils.RepresentsParameter(child, "wp")) as IParameterNode;

            // Check that no other parameters exist in the tree.
            (root.Children.Count() == 4 && !root.Children.Any(child => child.Children.Any())).ShouldBeTrue("Only 4 parameters are needed for SAPS.");

            // Check parameter domains.
            (alpha.Domain is LogDomain).ShouldBeTrue("alpha should be distributed logarithmically.");
            TestUtils.CheckRange((LogDomain)alpha.Domain, "alpha", expectedMinimum: 1.01, expectedMaximum: 1.4);
            (rho.Domain is ContinuousDomain).ShouldBeTrue("rho should be distributed uniformly.");
            TestUtils.CheckRange((ContinuousDomain)rho.Domain, "rho", expectedMinimum: 0, expectedMaximum: 1);
            (ps.Domain is ContinuousDomain).ShouldBeTrue("ps should be distributed uniformly.");
            TestUtils.CheckRange((ContinuousDomain)ps.Domain, "ps", 0, 0.2);
            (wp.Domain is ContinuousDomain).ShouldBeTrue("wp should be distributed uniformly.");
            TestUtils.CheckRange((ContinuousDomain)wp.Domain, "wp", 0, 0.06);
        }

        /// <summary>
        /// Checks that <see cref="SapsUtils.SeedsToUse"/> returns the correct number of seeds.
        /// </summary>
        [Fact]
        public void SeedsToUseReturnsCorrectNumberOfSeeds()
        {
            var numberOfSeeds = 6;
            var seedsToUse = SapsUtils.SeedsToUse(numberOfSeeds, 42);
            seedsToUse.Count().ShouldBe(numberOfSeeds);
        }

        /// <summary>
        /// Verifies that calling <see cref="SapsUtils.CreateInstances"/> with a non existant directory throws
        /// a <see cref="DirectoryNotFoundException"/>.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsExceptionIfItCannotOpenFolder()
        {
            Exception exception =
                Assert.Throws<DirectoryNotFoundException>(
                    () => { SapsUtils.CreateInstances("foobarFolder", 1, 42); });
        }

        /// <summary>
        /// Verifies that calling <see cref="SapsUtils.CreateInstances"/> with a non existant directory prints
        /// out a message to the console telling the user the directory doesn't exist.
        /// </summary>
        [Fact]
        public void CreateInstancesPrintsMessageIfItCannotOpenFolder()
        {
            TestUtils.CheckOutput(
                action: () =>
                    {
                        // Call CreateInstances with a non existant directory path.
                        try
                        {
                            SapsUtils.CreateInstances("foobarFolder", 1, 42);
                        }
                        catch (DirectoryNotFoundException)
                        {
                            // This is expected.
                        }
                    },
                check: consoleOutput =>
                    {
                        // Check that information about it is written to console.
                        StringReader reader = new StringReader(consoleOutput.ToString());
                        reader.ReadLine().ShouldContain("foobarFolder", "The problematic path did not get printed.");
                        reader.ReadLine().ShouldBe("Cannot open folder.", "Cause of exception has not been printed.");
                    });
        }

        /// <summary>
        /// Checks that <see cref="SapsUtils.CreateInstances"/> creates an instance out of each .cnf file and
        /// the instance's file name matches the complete path to that file.
        /// </summary>
        [Fact]
        public void CreateInstancesCorrectlyExtractsPathsToCnfFiles()
        {
            // Call method.
            var instances = SapsUtils.CreateInstances(this._instanceFolder, 1, 42);

            // Check that file names of instances match the complete paths of all .cnf files.
            var expectedPaths = SapsUtilsTests.CnfFileNames.Select(name => this._instanceFolder + Path.DirectorySeparatorChar + name);
            var instancePaths = instances.Select(instance => instance.Path);
            expectedPaths.ShouldBe(
                instancePaths,
                true,
                $"{TestUtils.PrintList(instancePaths)} should have been equal to {TestUtils.PrintList(expectedPaths)}.");
        }

        /// <summary>
        /// Checks that <see cref="SapsUtils.CreateInstances"/> ignores files which are not in .cnf format.
        /// </summary>
        [Fact]
        public void CreateInstancesIgnoresFilesNotInCnfFormat()
        {
            // Call method.
            var instances = SapsUtils.CreateInstances(this._instanceFolder, 1, 42);

            // Check that no non-cnf file has been translated into an instance.
            var instancePaths = instances.Select(instance => instance.Path);
            instancePaths.Any(path => SapsUtilsTests.NonCnfFileNames.Any(file => path.Contains(file)))
                .ShouldBeFalse("Not all non-cnf files have been ignored.");
        }

        #endregion
    }
}