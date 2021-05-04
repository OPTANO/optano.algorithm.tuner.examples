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

namespace Optano.Algorithm.Tuner.AcLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Optano.Algorithm.Tuner.AcLib.ParameterConfigurationSpace.Specification;
    using Optano.Algorithm.Tuner.Logging;
    using Optano.Algorithm.Tuner.Parameters;
    using Optano.Algorithm.Tuner.Parameters.ParameterTreeNodes;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    /// <summary>
    /// Utility methods for starting a tuner instance that tunes ACLib target algorithms.
    /// </summary>
    public static class AcLibUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// Creates a flat <see cref="ParameterTree"/> using the specified configuration.
        /// </summary>
        /// <param name="parameterConfiguration">The parameter configuration.</param>
        /// <returns>The created <see cref="ParameterTree"/>.</returns>
        public static ParameterTree CreateParameterTree(ParameterConfigurationSpaceSpecification parameterConfiguration)
        {
            if (parameterConfiguration == null)
            {
                throw new ArgumentNullException(nameof(parameterConfiguration));
            }

            var root = new AndNode();
            foreach (var parameter in parameterConfiguration.Parameters)
            {
                root.AddChild(parameter);
            }

            return new ParameterTree(root);
        }

        /// <summary>
        /// Creates the list of <see cref="InstanceSeedFile"/>s to train on using a specification file.
        /// </summary>
        /// <param name="pathToInstanceFile">
        /// Path to a text file specifying instances.
        /// Each line is of format "seed instance_file_path", where seed is an unsigned integer.
        /// </param>
        /// <returns>The created list. Notice that <see cref="InstanceSeedFile"/>s specify seeds as signed integers,
        /// so the instance file's seeds got transformed.</returns>
        public static List<InstanceSeedFile> CreateInstances(string pathToInstanceFile)
        {
            var specifications = File.ReadAllLines(pathToInstanceFile);

            var instances = new List<InstanceSeedFile>();
            foreach (var specification in specifications)
            {
                try
                {
                    var specificationParts = specification.Split(' ');
                    if (specificationParts.Length != 2)
                    {
                        throw new ArgumentException(
                            $"Line '{specification}' is not of format '<seed> <instance_file_path>'.");
                    }

                    // By AcLib specification, seeds are unsigned integers, but OPTANO Algorithm Tuner
                    // uses signed integers --> transform.
                    var seed = uint.Parse(specificationParts[0]);
                    int integerSeed;
                    unchecked
                    {
                        integerSeed = (int)seed;
                    }

                    instances.Add(new InstanceSeedFile(specificationParts[1], integerSeed));
                }
                catch (Exception e)
                {
                    LoggingHelper.WriteLine(VerbosityLevel.Warn, $"Could not parse instance '{specification}'.");
                    LoggingHelper.WriteLine(VerbosityLevel.Warn, e.Message);
                    throw;
                }
            }

            return instances;
        }

        #endregion
    }
}