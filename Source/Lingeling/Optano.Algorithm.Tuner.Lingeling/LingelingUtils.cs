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

namespace Optano.Algorithm.Tuner.Lingeling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.ParameterTreeReader;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    /// Utility methods for starting a tuner instance that tunes Lingeling.
    /// </summary>
    public static class LingelingUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// Creates a parameter tree from the "lingelingParamTree.xml" in the working directory.
        /// </summary>
        /// <returns>The <see cref="ParameterTree"/>.</returns>
        public static ParameterTree CreateParameterTree()
        {
            var parameterTree = ParameterTreeConverter.ConvertToParameterTree(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lingelingParamTree.xml"));
            LingelingUtils.AddAllActiveParameterWrappers(parameterTree);
            return parameterTree;
        }

        /// <summary>
        /// Creates the list of SAT instances to train on using .cnf files in the given directory.
        /// </summary>
        /// <param name="pathToInstanceFolder">Path to the folder containing the instance .cnf files.</param>
        /// <param name="numSeedsToUse">The number of seeds to use.</param>
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
                    foreach (var seed in LingelingUtils.SeedsToUse(numSeedsToUse, rngSeed))
                    {
                        instanceSeedFiles.Add(new InstanceSeedFile(instanceFilePath.FullName, seed));
                        fileAndSeedCsv += $";{seed}";
                    }

                    instanceSeedCombinations.Add(fileAndSeedCsv);
                }

                LingelingUtils.DumpFileSeedCombinations(instanceDirectory, instanceSeedCombinations);
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
                $"lingelingFileSeedCombinations_{DateTime.Now:MM-dd-hh-mm-ss}.csv");
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

        /// <summary>
        /// Adds all required parameter replacements to the <paramref name="parameterTree"/>.
        /// </summary>
        /// <param name="parameterTree">
        /// The parameter tree.
        /// </param>
        private static void AddAllActiveParameterWrappers(ParameterTree parameterTree)
        {
            parameterTree.AddParameterReplacementDefinition("bcamaxeffActive", false, "bcamaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("bcaminuseActive", false, "bcaminuse", 0, true);
            parameterTree.AddParameterReplacementDefinition("bkwdocclimActive", false, "bkwdocclim", 0, true);
            parameterTree.AddParameterReplacementDefinition("blkboostvlimActive", false, "blkboostvlim", 0, true);
            parameterTree.AddParameterReplacementDefinition("blkmaxeffActive", "unlimited", "blkmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("blkmaxeffActive", "disabled", "blkmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("blkmineffActive", false, "blkmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("cardmaxeffActive", "unlimited", "cardmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("cardmaxeffActive", "disabled", "cardmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("cardmaxlenActive", false, "cardmaxlen", 0, true);
            parameterTree.AddParameterReplacementDefinition("cardmineffActive", false, "cardmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("cardminlenActive", false, "cardminlen", 0, true);
            parameterTree.AddParameterReplacementDefinition("cardocclim1Active", false, "cardocclim1", 0, true);
            parameterTree.AddParameterReplacementDefinition("cardocclim2Active", false, "cardocclim2", 0, true);
            parameterTree.AddParameterReplacementDefinition("cce2waitActive", false, "cce2wait", 0, true);
            parameterTree.AddParameterReplacementDefinition("cce3waitActive", false, "cce3wait", 0, true);
            parameterTree.AddParameterReplacementDefinition("cceboostvlimActive", false, "cceboostvlim", 0, true);
            parameterTree.AddParameterReplacementDefinition("ccemaxeffActive", "unlimited", "ccemaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("ccemaxeffActive", "disabled", "ccemaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("ccemaxroundActive", false, "ccemaxround", 0, true);
            parameterTree.AddParameterReplacementDefinition("ccemineffActive", false, "ccemineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("decolimActive", false, "decolim", 0, true);
            parameterTree.AddParameterReplacementDefinition("elmaxeffActive", "unlimited", "elmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("elmaxeffActive", "disabled", "elmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("elmineffActive", false, "elmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("elmlitslimActive", false, "elmlitslim", 0, true);
            parameterTree.AddParameterReplacementDefinition("gaussmaxeffActive", "unlimited", "gaussmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("gaussmaxeffActive", "disabled", "gaussmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("gaussmineffActive", false, "gaussmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("locsActive", "unlimited", "locs", -1, true);
            parameterTree.AddParameterReplacementDefinition("locsActive", "disabled", "locs", 0, true);
            parameterTree.AddParameterReplacementDefinition("locsclimActive", false, "locsclim", 0, true);
            parameterTree.AddParameterReplacementDefinition("locsmaxeffActive", false, "locsmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("locsmineffActive", false, "locsmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("minlocalgluelimActive", false, "minlocalgluelim", 0, true);
            parameterTree.AddParameterReplacementDefinition("minlocalsizelimActive", false, "minlocalsizelim", 0, true);
            parameterTree.AddParameterReplacementDefinition("minrecgluelimActive", false, "minrecgluelim", 0, true);
            parameterTree.AddParameterReplacementDefinition("minrecsizelimActive", false, "minrecsizelim", 0, true);
            parameterTree.AddParameterReplacementDefinition("phaseluckmaxroundActive", false, "phaseluckmaxround", 0, true);
            parameterTree.AddParameterReplacementDefinition("prbasicmaxeffActive", "unlimited", "prbasicmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("prbasicmaxeffActive", "disabled", "prbasicmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("prbasicmineffActive", false, "prbasicmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("prbsimplemaxeffActive", "unlimited", "prbsimplemaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("prbsimplemaxeffActive", "disabled", "prbsimplemaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("prbsimplemineffActive", false, "prbsimplemineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("redclsglueActive", false, "redclsglue", 0, true);
            parameterTree.AddParameterReplacementDefinition("redclsizeActive", false, "redclsize", 0, true);
            parameterTree.AddParameterReplacementDefinition("redclsmaxlrgActive", false, "redclsmaxlrg", 0, true);
            parameterTree.AddParameterReplacementDefinition("redclsmaxpropsActive", false, "redclsmaxprops", 0, true);
            parameterTree.AddParameterReplacementDefinition("restartblockboundActive", false, "restartblockbound", 0, true);
            parameterTree.AddParameterReplacementDefinition("retireminActive", false, "retiremin", 0, true);
            parameterTree.AddParameterReplacementDefinition("simpcintdelayActive", false, "simpcintdelay", 0, true);
            parameterTree.AddParameterReplacementDefinition("simpcintmaxhardActive", "unlimited", "simpcintmaxhard", -1, true);
            parameterTree.AddParameterReplacementDefinition("simpcintmaxhardActive", "disabled", "simpcintmaxhard", 0, true);
            parameterTree.AddParameterReplacementDefinition("simpcintmaxsoftActive", "unlimited", "simpcintmaxsoft", -1, true);
            parameterTree.AddParameterReplacementDefinition("simpcintmaxsoftActive", "disabled", "simpcintmaxsoft", 0, true);
            parameterTree.AddParameterReplacementDefinition("simpincdelmaxminActive", false, "simpincdelmaxmin", 0, true);
            parameterTree.AddParameterReplacementDefinition("simpinitdelayActive", false, "simpinitdelay", 0, true);
            parameterTree.AddParameterReplacementDefinition("simpvarlimActive", false, "simpvarlim", 0, true);
            parameterTree.AddParameterReplacementDefinition("sweepforgiveActive", false, "sweepforgive", 0, true);
            parameterTree.AddParameterReplacementDefinition("sweepmaxdecActive", false, "sweepmaxdec", 0, true);
            parameterTree.AddParameterReplacementDefinition("sweepmaxeffActive", "unlimited", "sweepmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("sweepmaxeffActive", "disabled", "sweepmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("sweepmaxroundActive", "unlimited", "sweepmaxround", -1, true);
            parameterTree.AddParameterReplacementDefinition("sweepmaxroundActive", "disabled", "sweepmaxround", 0, true);
            parameterTree.AddParameterReplacementDefinition("sweepmindecActive", false, "sweepmindec", 0, true);
            parameterTree.AddParameterReplacementDefinition("sweepmineffActive", false, "sweepmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("synclsglueActive", false, "synclsglue", 0, true);
            parameterTree.AddParameterReplacementDefinition("synclslenActive", false, "synclslen", 0, true);
            parameterTree.AddParameterReplacementDefinition("trdmaxeffActive", "unlimited", "trdmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("trdmaxeffActive", "disabled", "trdmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("trdmineffActive", false, "trdmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("treelookmaxeffActive", "unlimited", "treelookmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("treelookmaxeffActive", "disabled", "treelookmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("treelookmineffActive", false, "treelookmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("trnrmaxeffActive", "unlimited", "trnrmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("trnrmaxeffActive", "disabled", "trnrmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("trnrmineffActive", false, "trnrmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("unhdlnprActive", false, "unhdlnpr", 0, true);
            parameterTree.AddParameterReplacementDefinition("unhdmaxeffActive", "unlimited", "unhdmaxeff", -1, true);
            parameterTree.AddParameterReplacementDefinition("unhdmaxeffActive", "disabled", "unhdmaxeff", 0, true);
            parameterTree.AddParameterReplacementDefinition("unhdmineffActive", false, "unhdmineff", 0, true);
            parameterTree.AddParameterReplacementDefinition("waitmaxActive", "unlimited", "waitmax", -1, true);
            parameterTree.AddParameterReplacementDefinition("waitmaxActive", "disabled", "waitmax", 0, true);
        }

        #endregion
    }
}