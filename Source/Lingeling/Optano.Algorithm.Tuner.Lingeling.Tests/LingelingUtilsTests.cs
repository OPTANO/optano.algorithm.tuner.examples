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

namespace Optano.Algorithm.Tuner.Lingeling.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="LingelingUtils"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class LingelingUtilsTests : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// File names that should be translated into instances on <see cref="LingelingUtils.CreateInstances"/>.
        /// </summary>
        private static readonly string[] CnfFileNames = { "useful1.cnf", "useful2.cnf" };

        /// <summary>
        /// File names that should not be translated into instances on <see cref="LingelingUtils.CreateInstances"/>.
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
        /// Initializes a new instance of the <see cref="LingelingUtilsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public LingelingUtilsTests()
        {
            this._instanceFolder = PathUtils.GetAbsolutePathFromExecutableFolderRelative("testData");
            Directory.CreateDirectory(this._instanceFolder);
            foreach (var fileName in LingelingUtilsTests.CnfFileNames.Union(LingelingUtilsTests.NonCnfFileNames))
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
        /// Checks that <see cref="LingelingUtils.SeedsToUse"/> returns the correct number of seeds.
        /// </summary>
        [Fact]
        public void SeedsToUseReturnsCorrectNumberOfSeeds()
        {
            var numberOfSeeds = 6;
            var seedsToUse = LingelingUtils.SeedsToUse(numberOfSeeds, 42);
            seedsToUse.Count().ShouldBe(numberOfSeeds);
        }

        /// <summary>
        /// Verifies that calling <see cref="LingelingUtils.CreateInstances"/> with a non existant directory throws
        /// a <see cref="DirectoryNotFoundException"/>.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsExceptionIfItCannotOpenFolder()
        {
            Exception exception =
                Assert.Throws<DirectoryNotFoundException>(
                    () => { LingelingUtils.CreateInstances("foobarFolder", 1, 42); });
        }

        /// <summary>
        /// Verifies that calling <see cref="LingelingUtils.CreateInstances"/> with a non existant directory prints
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
                            LingelingUtils.CreateInstances("foobarFolder", 1, 42);
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
        /// Checks that <see cref="LingelingUtils.CreateInstances"/> creates an instance out of each .cnf file and
        /// the instance's file name matches the complete path to that file.
        /// </summary>
        [Fact]
        public void CreateInstancesCorrectlyExtractsPathsToCnfFiles()
        {
            // Call method.
            var instances = LingelingUtils.CreateInstances(this._instanceFolder, 1, 42);

            // Check that file names of instances match the complete paths of all .cnf files.
            var expectedPaths = LingelingUtilsTests.CnfFileNames.Select(name => new FileInfo(Path.Combine(this._instanceFolder, name)).FullName);
            var instancePaths = instances.Select(instance => new FileInfo(instance.Path).FullName);
            instancePaths.ShouldBe(
                expectedPaths,
                true,
                $"{TestUtils.PrintList(instancePaths)} should have been equal to {TestUtils.PrintList(expectedPaths)}.");
        }

        /// <summary>
        /// Checks that <see cref="LingelingUtils.CreateInstances"/> ignores files which are not in .cnf format.
        /// </summary>
        [Fact]
        public void CreateInstancesIgnoresFilesNotInCnfFormat()
        {
            // Call method.
            var instances = LingelingUtils.CreateInstances(this._instanceFolder, 1, 42);

            // Check that no non-cnf file has been translated into an instance.
            var instancePaths = instances.Select(instance => instance.Path);
            instancePaths.Any(path => LingelingUtilsTests.NonCnfFileNames.Any(file => path.Contains(file)))
                .ShouldBeFalse("Not all non-cnf files have been ignored.");
        }

        #endregion
    }
}