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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using global::Gurobi;

    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.ParameterTreeReader;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    ///     Utility methods for starting an OPTANO Algorithm Tuner instance that tunes Gurobi.
    /// </summary>
    public class GurobiUtils
    {
        #region Static Fields

        /// <summary>
        /// Identifier of the parameter deciding whether the Gurobi parameter 'RINS' should be set automatically, to 0
        /// or by another gene.
        /// </summary>
        private static readonly string RinsActiveIdentifier = "RinsActive";

        /// <summary>
        /// Identifiers that identify nodes which have no directly corresponding Gurobi parameter.
        /// </summary>
        private static readonly string[] ArtificialNodeIdentifiers = new string[]
                                                                         {
                                                                             "AggFillActive",
                                                                             "CutPassesActive",
                                                                             "DegenMovesActive",
                                                                             "GomoryPassesActive",
                                                                             "MinRelNodesActive",
                                                                             "PrePassesActive",
                                                                             "PumpPassesActive",
                                                                             GurobiUtils.RinsActiveIdentifier,
                                                                             "ZeroObjNodesActive",
                                                                         };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates a <see cref="ParameterTree" /> of tuneable Gurobi parameters for MIP solving.
        ///     The parameters and their connections are described in parameterTree.xml.
        /// <para>
        /// Defines parameter replacements for artificial parameters.
        /// </para> 
        /// </summary>
        /// <returns>The <see cref="ParameterTree" />.</returns>
        public static ParameterTree CreateParameterTree()
        {
            var parameterTree = ParameterTreeConverter.ConvertToParameterTree(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"parameterTree.xml"));

            // handle special case: RINS
            parameterTree.AddParameterReplacementDefinition(GurobiUtils.RinsActiveIdentifier, 0, GRB.IntParam.RINS.ToString(), 0, true);
            parameterTree.AddParameterReplacementDefinition(GurobiUtils.RinsActiveIdentifier, -1, GRB.IntParam.RINS.ToString(), -1, true);

            // filter all ignored parameters
            foreach (var artificialNodeIdentifier in GurobiUtils.ArtificialNodeIdentifiers)
            {
                parameterTree.AddIgnoredParameter(artificialNodeIdentifier);
            }

            return parameterTree;
        }

        /// <summary>
        /// Creates the list of instances to train on using .mps files in the given directory.
        /// </summary>
        /// <param name="pathToInstanceFolder">Path to the folder containing the instance .mps files.</param>
        /// <param name="numSeedsToUse">The number of seeds.</param>
        /// <param name="rngSeed">The random number generator seed.</param>
        /// <returns>
        /// The created list.
        /// </returns>
        public static List<InstanceSeedFile> CreateInstances(string pathToInstanceFolder, int numSeedsToUse, int rngSeed)
        {
            try
            {
                // Find all .mps files in directory and set them as instances.
                var instanceDirectory = new DirectoryInfo(pathToInstanceFolder);
                var instanceSeedCombinations = new List<string>();
                var instanceSeedFiles = new List<InstanceSeedFile>();
                foreach (var instanceFilePath in instanceDirectory.EnumerateFiles()
                    .Where(file => file.Extension.ToLower() == ".mps"))
                {
                    var fileAndSeedCsv = instanceFilePath.FullName;
                    foreach (var seed in GurobiUtils.SeedsToUse(numSeedsToUse, rngSeed))
                    {
                        instanceSeedFiles.Add(new InstanceSeedFile(instanceFilePath.FullName, seed));
                        fileAndSeedCsv += $";{seed}";
                    }

                    instanceSeedCombinations.Add(fileAndSeedCsv);
                }

                GurobiUtils.DumpFileSeedCombinations(instanceDirectory, instanceSeedCombinations);
                return instanceSeedFiles;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine("Cannot open folder.");
                throw;
            }
        }

        /// <summary>
        /// Generates the seeds to use for the file seed combinations.
        /// </summary>
        /// <param name="numSeedsToUse">The number of seeds.</param>
        /// <param name="rngSeed">The random number generator seed.</param>
        /// <returns>The seeds.</returns>
        public static IEnumerable<int> SeedsToUse(int numSeedsToUse, int rngSeed)
        {
            var random = new Random(rngSeed);
            for (var i = 0; i < numSeedsToUse; i++)
            {
                yield return random.Next();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Dumps the file seed combinations.
        /// </summary>
        /// <param name="instanceDirectory">The instance directory.</param>
        /// <param name="instanceSeedCombinations">The instance seed combinations.</param>
        private static void DumpFileSeedCombinations(
            DirectoryInfo instanceDirectory,
            IEnumerable<string> instanceSeedCombinations)
        {
            var fileName = Path.Combine(
                instanceDirectory.FullName,
                $"gurobiFileSeedCombinations_{DateTime.Now:MM-dd-hh-mm-ss}.csv");
            try
            {
                File.WriteAllLines(fileName, instanceSeedCombinations, Encoding.UTF8);
            }
            catch (Exception e)
            {
                LoggingHelper.WriteLine(VerbosityLevel.Warn, $"Could not write Instance-Seed combinations to destination {fileName}");
                LoggingHelper.WriteLine(VerbosityLevel.Warn, e.Message);
            }
        }

        #endregion
    }
}