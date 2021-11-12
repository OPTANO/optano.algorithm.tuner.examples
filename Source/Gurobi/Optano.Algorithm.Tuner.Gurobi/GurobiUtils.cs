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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Gurobi.GurobiAdapterFeatures;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.ParameterTreeReader;

    using OPTANO.Modeling.Common;

    /// <summary>
    ///     Utility methods for starting an OPTANO Algorithm Tuner instance that tunes Gurobi.
    /// </summary>
    public static class GurobiUtils
    {
        #region Static Fields

        /// <summary>
        /// The list of valid file compression extensions of Gurobi.
        /// Note, that a file can also be uncompressed, resulting in an empty string.
        /// </summary>
        public static readonly string[] ListOfValidFileCompressionExtensions = { string.Empty, ".gz", ".bz2", ".7z" };

        /// <summary>
        /// The list of valid file extensions of Gurobi.
        /// </summary>
        public static readonly string[] ListOfValidFileExtensions =
            GurobiUtils.ListOfValidFileCompressionExtensions.Select(compressionExtension => ".mps" + compressionExtension).ToArray();

        /// <summary>
        /// Names for individual bit-values of the <c>PartitionPlace</c> parameter: https://www.gurobi.com/documentation/9.0/refman/partitionplace.html.
        /// </summary>
        internal static readonly HashSet<string> PartitionPlaceGroupIdentifiers = new HashSet<string>(
            new[]
                {
                    "PartitionPlace_BeforeRoot",
                    "PartitionPlace_StartOfCutLoop",
                    "PartitionPlace_EntOfCutLoop",
                    "PartitionPlace_NodesOfBaC",
                    "PartitionPlace_TerminationOfBaC",
                });

        /// <summary>
        /// Parameter name for the PartitionPlace heuristic.
        /// </summary>
        internal static readonly string PartitionPlaceName = "PartitionPlace";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Composes the given adapter features to a single array.
        /// </summary>
        /// <param name="currentRuntimeFeatures">The current runtime features.</param>
        /// <param name="lastRuntimeFeatures">The last runtime features.</param>
        /// <param name="instanceFeatures">The instance features.</param>
        /// <returns>The composed array.</returns>
        public static double[] ComposeAdapterFeatures(
            GurobiRuntimeFeatures currentRuntimeFeatures,
            GurobiRuntimeFeatures lastRuntimeFeatures,
            GurobiInstanceFeatures instanceFeatures)
        {
            return currentRuntimeFeatures.ToArray()
                .Concat(lastRuntimeFeatures.ToArray())
                .Concat(instanceFeatures.ToArray())
                .ToArray();
        }

        /// <summary>
        /// Composes the header of the given adapter features to a single array.
        /// </summary>
        /// <param name="currentRuntimeFeatures">The current runtime features.</param>
        /// <param name="lastRuntimeFeatures">The last runtime features.</param>
        /// <param name="instanceFeatures">The instance features.</param>
        /// <returns>The composed array.</returns>
        public static string[] ComposeAdapterFeaturesHeader(
            GurobiRuntimeFeatures currentRuntimeFeatures,
            GurobiRuntimeFeatures lastRuntimeFeatures,
            GurobiInstanceFeatures instanceFeatures)
        {
            return currentRuntimeFeatures.GetHeader("RuntimeFeature_", "_Current")
                .Concat(lastRuntimeFeatures.GetHeader("RuntimeFeature_", "_Last"))
                .Concat(instanceFeatures.GetHeader("InstanceFeature_"))
                .ToArray();
        }

        /// <summary>
        /// Returns the file name without its extension, if the file has a valid Gurobi model extension.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>The file name without its extension.</returns>
        public static string GetFileNameWithoutGurobiExtension(FileInfo fileInfo)
        {
            foreach (var extension in GurobiUtils.ListOfValidFileExtensions)
            {
                if (fileInfo.Name.EndsWith(extension))
                {
                    return fileInfo.Name.TruncateToLength(fileInfo.Name.Length - extension.Length);
                }
            }

            throw new ArgumentException($"The given file {fileInfo.FullName} has not a valid Gurobi model extension.");
        }

        /// <summary>
        /// Creates a <see cref="ParameterTree" /> of tuneable Gurobi parameters for MIP solving.
        /// The parameters and their connections are described in parameterTree.xml.
        /// </summary>
        /// <returns>The <see cref="ParameterTree" />.</returns>
        public static ParameterTree CreateParameterTree()
        {
            var parameterTree = ParameterTreeConverter.ConvertToParameterTree(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? throw new InvalidOperationException(), @"parameterTree.xml"));
            GurobiUtils.AddAllIndicatorParameterWrappers(parameterTree);
            return parameterTree;
        }

        /// <summary>
        /// Returns the mip gap.
        /// </summary>
        /// <param name="bestObjective">The best objective.</param>
        /// <param name="bestObjectiveBound">The best objective bound.</param>
        /// <returns>The mip gap.</returns>
        public static double GetMipGap(double bestObjective, double bestObjectiveBound)
        {
            if (bestObjective.Equals(0) && bestObjectiveBound.Equals(0))
            {
                return 0;
            }

            if (bestObjective.Equals(0) || double.IsNaN(bestObjective) || double.IsNaN(bestObjectiveBound)
                || Math.Abs(bestObjective).Equals(GRB.INFINITY) || Math.Abs(bestObjectiveBound).Equals(GRB.INFINITY))
            {
                return GRB.INFINITY;
            }

            return Math.Abs(bestObjectiveBound - bestObjective) / Math.Abs(bestObjective);
        }

        /// <summary>
        /// Gets a fallback value for the best objective, dependent on <paramref name="optimizationSenseIsMinimize"/>.
        /// </summary>
        /// <param name="optimizationSenseIsMinimize">A value indicating whether the optimization sense is minimize.</param>
        /// <returns>The best objective fallback.</returns>
        public static double GetBestObjectiveFallback(bool optimizationSenseIsMinimize)
        {
            return optimizationSenseIsMinimize ? GRB.INFINITY : -GRB.INFINITY;
        }

        /// <summary>
        /// Gets a fallback value for the best objective bound, dependent on <paramref name="optimizationSenseIsMinimize"/>.
        /// </summary>
        /// <param name="optimizationSenseIsMinimize">A value indicating whether the optimization sense is minimize.</param>
        /// <returns>The best objective fallback.</returns>
        public static double GetBestObjectiveBoundFallback(bool optimizationSenseIsMinimize)
        {
            return -GurobiUtils.GetBestObjectiveFallback(optimizationSenseIsMinimize);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds all required parameter replacements to the <paramref name="parameterTree"/>.
        /// </summary>
        /// <param name="parameterTree">
        /// The parameter tree.
        /// </param>
        private static void AddAllIndicatorParameterWrappers(ParameterTree parameterTree)
        {
            parameterTree.AddParameterReplacementDefinition("AggFillIndicator", "off", GRB.IntParam.AggFill.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("AggFillIndicator", "default", GRB.IntParam.AggFill.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("CutPassesIndicator", "off", GRB.IntParam.CutPasses.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("CutPassesIndicator", "default", GRB.IntParam.CutPasses.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("DegenMovesIndicator", "off", GRB.IntParam.DegenMoves.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("DegenMovesIndicator", "default", GRB.IntParam.DegenMoves.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("GomoryPassesIndicator", "off", GRB.IntParam.GomoryPasses.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("GomoryPassesIndicator", "default", GRB.IntParam.GomoryPasses.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("MinRelNodesIndicator", "off", GRB.IntParam.MinRelNodes.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("MinRelNodesIndicator", "default", GRB.IntParam.MinRelNodes.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("PrePassesIndicator", "off", GRB.IntParam.PrePasses.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("PrePassesIndicator", "default", GRB.IntParam.PrePasses.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("PumpPassesIndicator", "off", GRB.IntParam.PumpPasses.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("PumpPassesIndicator", "default", GRB.IntParam.PumpPasses.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("RinsIndicator", "off", GRB.IntParam.RINS.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("RinsIndicator", "default", GRB.IntParam.RINS.ToString(), -1, true);
            parameterTree.AddParameterReplacementDefinition("ZeroObjNodesIndicator", "off", GRB.IntParam.ZeroObjNodes.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition("ZeroObjNodesIndicator", "default", GRB.IntParam.ZeroObjNodes.ToString(), -1, true);
        }

        #endregion
    }
}