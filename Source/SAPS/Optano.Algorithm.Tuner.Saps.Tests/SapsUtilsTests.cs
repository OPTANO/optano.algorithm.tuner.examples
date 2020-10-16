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
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SapsUtilsTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public SapsUtilsTests()
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc/>
        public void Dispose()
        {
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

        #endregion
    }
}