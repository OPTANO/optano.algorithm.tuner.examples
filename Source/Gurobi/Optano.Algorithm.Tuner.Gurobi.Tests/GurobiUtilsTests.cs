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
    /// Contains tests for the <see cref="GurobiUtils"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiUtilsTests : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// File names that should be translated into instances on <see cref="GurobiUtils.CreateInstances"/>.
        /// </summary>
        private static readonly string[] MpsFileNames = { "useful1.mps", "useful2.mps" };

        /// <summary>
        /// File names that should not be translated into instances on <see cref="GurobiUtils.CreateInstances"/>.
        /// </summary>
        private static readonly string[] NonMpsFileNames = { "useless.txt" };

        #endregion

        #region Fields

        /// <summary>
        /// Path to the folder containing test data. Has to be initialized.
        /// </summary>
        private readonly string _instanceFolder;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiUtilsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiUtilsTests()
        {
            this._instanceFolder = PathUtils.GetAbsolutePathFromExecutableFolderRelative("testData");
            Directory.CreateDirectory(this._instanceFolder);
            foreach (var fileName in GurobiUtilsTests.MpsFileNames.Union(GurobiUtilsTests.NonMpsFileNames))
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
        /// Checks that <see cref="GurobiUtils.SeedsToUse"/> returns the correct number of seeds.
        /// </summary>
        [Fact]
        public void SeedsToUseReturnsCorrectNumberOfSeeds()
        {
            var numberOfSeeds = 6;
            var seedsToUse = GurobiUtils.SeedsToUse(numberOfSeeds, 42);
            seedsToUse.Count().ShouldBe(numberOfSeeds);
        }

        /// <summary>
        /// Verifies that calling <see cref="GurobiUtils.CreateInstances"/> with a non existant directory throws
        /// a <see cref="DirectoryNotFoundException"/>.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsExceptionIfItCannotOpenFolder()
        {
            Exception exception =
                Assert.Throws<DirectoryNotFoundException>(
                    () => { GurobiUtils.CreateInstances("foobarFolder", 1, 42); });
        }

        /// <summary>
        /// Verifies that calling <see cref="GurobiUtils.CreateInstances"/> with a non existant directory prints
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
                            GurobiUtils.CreateInstances("foobarFolder", 1, 42);
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
        /// Checks that <see cref="GurobiUtils.CreateInstances"/> creates an instance out of each .mps file and
        /// the instance's file name matches the complete path to that file.
        /// </summary>
        [Fact]
        public void CreateInstancesCorrectlyExtractsPathsToMpsFiles()
        {
            // Call method.
            var instances = GurobiUtils.CreateInstances(this._instanceFolder, 1, 42);

            // Check that file names of instances match the complete paths of all .mps files.
            var expectedPaths = GurobiUtilsTests.MpsFileNames.Select(name => this._instanceFolder + Path.DirectorySeparatorChar + name);
            var instancePaths = instances.Select(instance => instance.Path);
            expectedPaths.ShouldBe(
                instancePaths,
                true,
                $"{TestUtils.PrintList(instancePaths)} should have been equal to {TestUtils.PrintList(expectedPaths)}.");
        }

        /// <summary>
        /// Checks that <see cref="GurobiUtils.CreateInstances"/> ignores files which are not in .mps format.
        /// </summary>
        [Fact]
        public void CreateInstancesIgnoresFilesNotInMpsFormat()
        {
            // Call method.
            var instances = GurobiUtils.CreateInstances(this._instanceFolder, 1, 42);

            // Check that no non-mps file has been translated into an instance.
            var instancePaths = instances.Select(instance => instance.Path);
            instancePaths.Any(path => GurobiUtilsTests.NonMpsFileNames.Any(file => path.Contains(file)))
                .ShouldBeFalse("Not all non-mps files have been ignored.");
        }

        #endregion
    }
}
