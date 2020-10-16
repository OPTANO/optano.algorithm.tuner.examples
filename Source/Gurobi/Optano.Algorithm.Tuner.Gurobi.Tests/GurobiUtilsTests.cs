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

    using Optano.Algorithm.Tuner.Configuration;
    using Optano.Algorithm.Tuner.Genomes;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="GurobiUtils"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiUtilsTests : IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiUtilsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiUtilsTests()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Checks, that <see cref="GurobiUtils.GetFileNameWithoutGurobiExtension"/> returns correct file name.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="extension">The extension.</param>
        [Theory]
        [InlineData("test", ".mps")]
        [InlineData("test", ".mps.gz")]
        [InlineData("test", ".mps.bz2")]
        [InlineData("test", ".mps.7z")]
        public void GetFileNameWithoutGurobiExtensionReturnsCorrectFileName(string fileName, string extension)
        {
            var fileInfo = new FileInfo(fileName + extension);
            var fileNameWithoutExtension = GurobiUtils.GetFileNameWithoutGurobiExtension(fileInfo);
            fileNameWithoutExtension.ShouldBe(fileName);
        }

        /// <summary>
        /// Checks, that <see cref="GurobiUtils.GetFileNameWithoutGurobiExtension"/> throws an <see cref="ArgumentException"/>, if the given file has not a valid Gurobi model extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        [Theory]
        [InlineData("test.mst")]
        [InlineData("test.mps.zip")]
        public void GetFileNameWithoutGurobiExtensionThrowsIfExtensionIsNotValid(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            Assert.Throws<ArgumentException>(() => GurobiUtils.GetFileNameWithoutGurobiExtension(fileInfo));
        }

        /// <summary>
        /// Checks that <see cref="GurobiUtils.CreateParameterTree"/> throws no exception.
        /// </summary>
        [Fact]
        public void CreateParameterTreeThrowsNoException()
        {
            try
            {
                var parameterTree = GurobiUtils.CreateParameterTree();
            }
            catch (Exception exception)
            {
                Assert.True(false, $"Exception: {exception.Message}");
            }
        }

        /// <summary>
        /// Checks that <see cref="GurobiUtils.CreateParameterTree"/> filters all dummy / indicator parameters.
        /// </summary>
        [Fact]
        public void AllDummyParametersAreFiltered()
        {
            Randomizer.Configure(0);
            var parameterTree = GurobiUtils.CreateParameterTree();
            var config = new AlgorithmTunerConfiguration.AlgorithmTunerConfigurationBuilder().Build(1);
            var genomeBuilder = new GenomeBuilder(parameterTree, config);

            var genome = genomeBuilder.CreateRandomGenome(0);

            var filteredGenes = genome.GetFilteredGenes(parameterTree);

            foreach (var filteredGenesKey in filteredGenes.Keys)
            {
                filteredGenesKey.ShouldNotContain("Indicator");
            }
        }

        #endregion
    }
}