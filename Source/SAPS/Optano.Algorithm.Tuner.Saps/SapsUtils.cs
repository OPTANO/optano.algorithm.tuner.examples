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

namespace Optano.Algorithm.Tuner.Saps
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

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
                new LogDomain(minimum: 1.01, maximum: 1.4));
            var rhoNode = new ValueNode<double>(
                SapsUtils.RhoIdentifier,
                new ContinuousDomain(minimum: 0, maximum: 1));
            var pSmoothNode = new ValueNode<double>(
                SapsUtils.PSmoothIdentifier,
                new ContinuousDomain(minimum: 0, maximum: 0.2));
            var wpNode = new ValueNode<double>(
                SapsUtils.WpIdentifier,
                new ContinuousDomain(minimum: 0, maximum: 0.06));

            var rootNode = new AndNode();
            rootNode.AddChild(alphaNode);
            rootNode.AddChild(rhoNode);
            rootNode.AddChild(pSmoothNode);
            rootNode.AddChild(wpNode);

            return new ParameterTree(rootNode);
        }

        /// <summary>
        /// Creates the list of SAT instances to train on using .cnf files in the given directory.
        /// </summary>
        /// <param name="pathToInstanceFolder">Path to the folder containing the instance .cnf files.</param>
        /// <param name="numSeedsToUse">The number of seeds.</param>
        /// <param name="rngSeed">The random number generator seed.</param>
        /// <returns>
        /// The created list.
        /// </returns>
        public static List<InstanceSeedFile> CreateInstances(string pathToInstanceFolder, int numSeedsToUse, int rngSeed)
        {
            try
            {
                // Find all .cnf files in directory and set them as instances.
                var instanceDirectory = new DirectoryInfo(pathToInstanceFolder);
                var instanceSeedCombinations = new List<string>();
                var instanceSeedFiles = new List<InstanceSeedFile>();
                foreach (var instanceFilePath in instanceDirectory.EnumerateFiles()
                    .Where(file => file.Extension.ToLower() == ".cnf"))
                {
                    var fileAndSeedCsv = instanceFilePath.FullName;
                    foreach (var seed in SapsUtils.SeedsToUse(numSeedsToUse, rngSeed))
                    {
                        instanceSeedFiles.Add(new InstanceSeedFile(instanceFilePath.FullName, seed));
                        fileAndSeedCsv += $";{seed}";
                    }

                    instanceSeedCombinations.Add(fileAndSeedCsv);
                }

                SapsUtils.DumpFileSeedCombinations(instanceDirectory, instanceSeedCombinations);
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
                $"sapsFileSeedCombinations_{DateTime.Now:MM-dd-hh-mm-ss}.csv");
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