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

namespace Optano.Algorithm.Tuner.Saps
{
    using System.Diagnostics.CodeAnalysis;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;

    /// <summary>
    /// Utility methods for starting a tuner instance that tunes SAPS.
    /// </summary>
    public static class SapsUtils
    {
        #region Constants

        /// <summary>
        /// Identifier of the SAPS parameter which is known as alpha.
        /// </summary>
        public const string AlphaIdentifier = "alpha";

        /// <summary>
        /// Identifier of the SAPS parameter which is known as rho.
        /// </summary>
        public const string RhoIdentifier = "rho";

        /// <summary>
        /// Identifier of the SAPS parameter which is known as P_smooth.
        /// </summary>
        public const string PSmoothIdentifier = "ps";

        /// <summary>
        /// Identifier of the SAPS parameter which is known as wp.
        /// </summary>
        public const string WpIdentifier = "wp";

        #endregion

        #region Static Fields

        /// <summary>
        /// The list of valid file extensions of SAPS.
        /// </summary>
        public static readonly string[] ListOfValidFileExtensions = { ".cnf" };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a <see cref="ParameterTree"/> consisting of four independent nodes:
        /// * one representing the continuous parameter with name <see cref="AlphaIdentifier"/>,
        /// * one representing the continuous parameter with name <see cref="RhoIdentifier"/>,
        /// * one representing the continuous parameter with name <see cref="PSmoothIdentifier"/>, and
        /// * one representing the continuous parameter with name <see cref="WpIdentifier"/>.
        /// </summary>
        /// <returns>The <see cref="ParameterTree"/>.</returns>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "SAPS parameters are very short and may look like hungarian notation, but they are not.")]
        public static ParameterTree CreateParameterTree()
        {
            var alphaNode = new ValueNode<double>(
                SapsUtils.AlphaIdentifier,
                new LogDomain(minimum: 1.01, maximum: 1.4, new Allele<double>(1.3)));
            var rhoNode = new ValueNode<double>(
                SapsUtils.RhoIdentifier,
                new ContinuousDomain(minimum: 0, maximum: 1, new Allele<double>(0.8)));
            var pSmoothNode = new ValueNode<double>(
                SapsUtils.PSmoothIdentifier,
                new ContinuousDomain(minimum: 0, maximum: 0.2, new Allele<double>(0.05)));
            var wpNode = new ValueNode<double>(
                SapsUtils.WpIdentifier,
                new ContinuousDomain(minimum: 0, maximum: 0.06, new Allele<double>(0.01)));

            var rootNode = new AndNode();
            rootNode.AddChild(alphaNode);
            rootNode.AddChild(rhoNode);
            rootNode.AddChild(pSmoothNode);
            rootNode.AddChild(wpNode);

            return new ParameterTree(rootNode);
        }

        #endregion
    }
}