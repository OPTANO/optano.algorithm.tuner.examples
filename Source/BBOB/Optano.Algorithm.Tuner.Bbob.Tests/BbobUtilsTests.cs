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

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="BbobUtils"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class BbobUtilsTests : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// The instance folder used in tests.
        /// </summary>
        private static readonly string InstanceFolder = "instances/train";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BbobUtilsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public BbobUtilsTests()
        {
            this.ClearInstanceFolder();
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
            this.ClearInstanceFolder();
        }

        /// <summary>
        /// Checks that the number of created instances fits to the number of created files.
        /// </summary>
        [Fact]
        public void NumberOfCreatedInstancesFitsToNumberOfCreatedFiles()
        {
            const int NumberOfInstances = 42;
            BbobUtils.CreateInstancesFilesAndReturnAsList(BbobUtilsTests.InstanceFolder, NumberOfInstances, new Random(0));
            var createdInstances = Directory.GetFiles(BbobUtilsTests.InstanceFolder, "*.bbi");
            createdInstances.Length.ShouldBe(NumberOfInstances, "Expected different number of instances");
        }

        /// <summary>
        /// Checks that the <see cref="BbobUtils.CreateInstancesFilesAndReturnAsList"/> method creates no new instance files, if there are enough instance files in the instance folder.
        /// </summary>
        [Fact]
        public void CreateNoNewInstanceFilesIfNotNecessary()
        {
            const int NumberOfOldInstances = 20;
            const int NumberOfNewInstances = 10;
            BbobUtils.CreateInstancesFilesAndReturnAsList(BbobUtilsTests.InstanceFolder, NumberOfOldInstances, new Random(0));
            BbobUtils.CreateInstancesFilesAndReturnAsList(BbobUtilsTests.InstanceFolder, NumberOfNewInstances, new Random(0));
            var createdInstances = Directory.GetFiles(BbobUtilsTests.InstanceFolder, "*.bbi");
            createdInstances.Length.ShouldBe(NumberOfOldInstances, "Expected different number of instances");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the instance folder.
        /// </summary>
        private void ClearInstanceFolder()
        {
            if (!Directory.Exists(BbobUtilsTests.InstanceFolder))
            {
                return;
            }

            var filesToDelete = Directory.GetFiles(BbobUtilsTests.InstanceFolder, "*.bbi");
            foreach (var file in filesToDelete)
            {
                File.Delete(file);
            }
        }

        #endregion
    }
}