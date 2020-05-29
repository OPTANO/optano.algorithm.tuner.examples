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

namespace Optano.Algorithm.Tuner.Bbob
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.Domains;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    ///     Utility methods for starting an OPTANO Algorithm Tuner instance that tunes BBOB.
    /// </summary>
    public class BbobUtils
    {
        #region Static Fields

        /// <summary>
        /// Template for naming BBOB parameters.
        /// </summary>
        public static readonly string IdentifierTemplate = "x{0}";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a <see cref="ParameterTree"/> consisting of <paramref name="parameterCount"/> independent nodes:
        /// * x0 through x{<paramref name="parameterCount"/> - 1}.
        /// * All parameters range from -5 to 5, as all BBOB Functions will have their respective optimum in that range.
        /// </summary>
        /// <param name="parameterCount">The number of parameters to use for BBOB functions.</param>
        /// <returns>The <see cref="ParameterTree"/>.</returns>
        public static ParameterTree CreateParameterTree(int parameterCount)
        {
            if (parameterCount <= 0)
            {
                throw new ArgumentException($"{nameof(parameterCount)} must be positive.", nameof(parameterCount));
            }

            var root = new AndNode();
            for (var i = 0; i < parameterCount; i++)
            {
                var node = new ValueNode<double>(string.Format(BbobUtils.IdentifierTemplate, i), new ContinuousDomain(minimum: -5, maximum: 5));
                root.AddChild(node);
            }

            return new ParameterTree(root);
        }

        /// <summary>
        /// Creates the instances files and return it as list.
        /// </summary>
        /// <param name="pathToInstanceFolder">The path to instance folder.</param>
        /// <param name="requiredInstanceNumber">The required instance number.</param>
        /// <param name="random">The random generator for seeds.</param>
        /// <returns>The instance files as list.</returns>
        public static List<InstanceFile> CreateInstancesFilesAndReturnAsList(string pathToInstanceFolder, int requiredInstanceNumber, Random random)
        {
            BbobUtils.CreateInstanceFiles(pathToInstanceFolder, requiredInstanceNumber, random);
            return BbobUtils.CreateInstanceList(pathToInstanceFolder);
        }

        /// <summary>
        /// Creates the list of <see cref="InstanceFile"/>s to train on using .cnf files in the given directory.
        /// </summary>
        /// <param name="pathToInstanceFolder">Path to the folder containing the instance .cnf files.</param>
        /// <returns>The created list.</returns>
        public static List<InstanceFile> CreateInstanceList(string pathToInstanceFolder = "DummyInstances")
        {
            try
            {

                // Find all .bbi files in directory and set them as instances.
                var instanceDirectory = new DirectoryInfo(pathToInstanceFolder);
                return
                    instanceDirectory.EnumerateFiles()
                                     .Where(file => file.Extension.ToLower() == ".bbi")
                                     .Select(file => new InstanceFile(file.FullName))
                                     .ToList();
            }
            catch (Exception e)
            {
                // Echo information and rethrow exception if that was not possible.
                LoggingHelper.WriteLine(VerbosityLevel.Warn, e.Message);
                LoggingHelper.WriteLine(VerbosityLevel.Warn, "Cannot open folder.");
                throw;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the instance files.
        /// </summary>
        /// <param name="pathToInstanceFolder">The instance folder path.</param>
        /// <param name="requiredInstanceNumber">The required number of instances.</param>
        /// <param name="random">The random generator for seeds.</param>
        private static void CreateInstanceFiles(string pathToInstanceFolder, int requiredInstanceNumber, Random random)
        {
            if (!Directory.Exists(pathToInstanceFolder))
            {
                LoggingHelper.WriteLine(VerbosityLevel.Debug, $"Creating directory: {pathToInstanceFolder}");
                Directory.CreateDirectory(pathToInstanceFolder);
            }

            var nameTemplate = Path.Combine(pathToInstanceFolder, "Instance{0}.bbi");
            var fileInfos = Enumerable.Range(0, requiredInstanceNumber).Select(
                i =>
                    new { Content = $"{random.Next()}", FileInfo = new FileInfo(string.Format(nameTemplate, i)) });

            var fullInstanceFolderPath = Path.GetFullPath(pathToInstanceFolder);
            var existingInstances = Directory.GetFiles(fullInstanceFolderPath, "*", SearchOption.TopDirectoryOnly)
                .Count(f => f.EndsWith(".bbi"));

            // make sure to use existing instances.
            // only add new instances, if they are really required.
            if (requiredInstanceNumber > existingInstances)
            {
                foreach (var file in fileInfos)
                {
                    // stop creating new instances, if the number of existing instances suffices.
                    if (File.Exists(file.FileInfo.FullName) || requiredInstanceNumber <= existingInstances)
                    {
                        continue;
                    }

                    existingInstances++;
                    File.WriteAllText(file.FileInfo.FullName, file.Content);
                }

                // we only enter the if-statement, when new files had to be created. check will not fail, if initial instance count already exceeded the required instance count.
                Debug.Assert(requiredInstanceNumber == existingInstances, "requiredInstances should equal existingInstances.");
            }
        }

        #endregion
    }
}
