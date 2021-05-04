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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="AcLibUtils"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class AcLibUtilsTest : IDisposable
    {
        #region Constants

        /// <summary>
        /// Path for file specifying seeds and instance files.
        /// </summary>
        private const string InstanceSpecificationFile = "instances.txt";

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
            if (File.Exists(AcLibUtilsTest.InstanceSpecificationFile))
            {
                File.Delete(AcLibUtilsTest.InstanceSpecificationFile);
            }
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateParameterTree"/> throws a <see cref="ArgumentNullException"/> if
        /// called without a <see cref="ParameterConfigurationSpaceSpecification"/>.
        /// </summary>
        [Fact]
        public void CreateParameterTreeThrowsForMissingSpecification()
        {
            Assert.Throws<ArgumentNullException>(() => AcLibUtils.CreateParameterTree(parameterConfiguration: null));
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateParameterTree"/> creates a flat tree consisting of exactly the
        /// specified parameters.
        /// </summary>
        [Fact]
        public void CreateParameterTreeCreatesFlatTree()
        {
            var parameters = new List<IParameterNode>
                                 {
                                     new ValueNode<string>("a", new CategoricalDomain<string>(new List<string> { "0", "1" })),
                                     new ValueNode<int>("b", new DiscreteLogDomain(1, 1024)),
                                     new ValueNode<double>("c", new ContinuousDomain(-1.02, 2.6)),
                                 };
            var specification = new ParameterConfigurationSpaceSpecification(
                parameters,
                new Dictionary<string, List<EqualsCondition>>(),
                new List<ForbiddenParameterCombination>());
            var tree = AcLibUtils.CreateParameterTree(specification);

            Assert.Equal(3, tree.GetParameters().Count());
            foreach (var parameter in parameters)
            {
                Assert.True(
                    tree.Root.Children.Contains(parameter),
                    $"{parameter} was not placed directly below the parameter tree root.");
            }
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateInstances"/> throws a <see cref="FileNotFoundException"/> if
        /// called with a non-existent instance file.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsForMissingInstanceFile()
        {
            Assert.Throws<FileNotFoundException>(() => AcLibUtils.CreateInstances(AcLibUtilsTest.InstanceSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateInstances"/> throws a <see cref="ArgumentException"/> if
        /// the instance file contains a line which does contains less than two space-separated strings.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsForInsufficientInformation()
        {
            AcLibUtilsTest.WriteFile(new[] { "0 a", "b" }, AcLibUtilsTest.InstanceSpecificationFile);
            Assert.Throws<ArgumentException>(() => AcLibUtils.CreateInstances(AcLibUtilsTest.InstanceSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateInstances"/> throws a <see cref="ArgumentException"/> if
        /// the instance file contains a line which does contains more than two space-separated strings.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsForTooMuchInformation()
        {
            AcLibUtilsTest.WriteFile(new[] { "0 a", "330 b x" }, AcLibUtilsTest.InstanceSpecificationFile);
            Assert.Throws<ArgumentException>(() => AcLibUtils.CreateInstances(AcLibUtilsTest.InstanceSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateInstances"/> throws a <see cref="FormatException"/> if
        /// the instance file contains a line in which the seed is not a number.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsForWrongSeedFormat()
        {
            AcLibUtilsTest.WriteFile(new[] { "0 a", "4f b" }, AcLibUtilsTest.InstanceSpecificationFile);
            Assert.Throws<FormatException>(() => AcLibUtils.CreateInstances(AcLibUtilsTest.InstanceSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateInstances"/> throws a <see cref="OverflowException"/> if
        /// the instance file contains a line in which the seed is negative.
        /// </summary>
        [Fact]
        public void CreateInstancesThrowsForNegativeSeed()
        {
            AcLibUtilsTest.WriteFile(new[] { "0 a", "-1 b" }, AcLibUtilsTest.InstanceSpecificationFile);
            Assert.Throws<OverflowException>(() => AcLibUtils.CreateInstances(AcLibUtilsTest.InstanceSpecificationFile));
        }

        /// <summary>
        /// Checks that <see cref="AcLibUtils.CreateInstances"/> correctly creates instances if the file is formatted
        /// correctly.
        /// </summary>
        [Fact]
        public void CreateInstancesReadsInstancesCorrectly()
        {
            AcLibUtilsTest.WriteFile(new[] { "330 ba", "2147483648 foo/bar" }, AcLibUtilsTest.InstanceSpecificationFile);
            var instances = AcLibUtils.CreateInstances(AcLibUtilsTest.InstanceSpecificationFile);

            Assert.Equal(2, instances.Count);
            Assert.Equal("ba", instances[0].Path);
            Assert.Equal(330, instances[0].Seed);
            Assert.Equal("foo/bar", instances[1].Path);
            Assert.Equal(int.MinValue, instances[1].Seed);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes a simple file.
        /// </summary>
        /// <param name="lines">The lines to write.</param>
        /// <param name="path">The path to write the file to.</param>
        private static void WriteFile(IEnumerable<string> lines, string path)
        {
            using var file = File.CreateText(path);
            foreach (var line in lines)
            {
                file.WriteLine(line);
            }
        }

        #endregion
    }
}