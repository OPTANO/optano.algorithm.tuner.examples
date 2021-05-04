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

namespace Optano.Algorithm.Tuner.AcLib.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Relevant data from an ACLib scenario file.
    /// </summary>
    public class Scenario
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Scenario"/> class.
        /// </summary>
        /// <param name="scenarioFile">The scenario file.</param>
        public Scenario(string scenarioFile)
        {
            if (!File.Exists(scenarioFile))
            {
                throw new FileNotFoundException($"There is no file at {scenarioFile}.", scenarioFile);
            }

            foreach (var line in File.ReadAllLines(scenarioFile))
            {
                var keyAndValue = line.Split('=').Select(part => part.Trim()).ToArray();
                switch (keyAndValue[0])
                {
                    case "algo":
                        this.Command = keyAndValue[1];
                        break;
                    case "cutoff_time":
                        if (!int.TryParse(keyAndValue[1], out var cutoffTime))
                        {
                            throw new FormatException($"Cutoff time {keyAndValue[1]} should be an integer.");
                        }

                        this.CutoffTime = TimeSpan.FromSeconds(cutoffTime);
                        break;
                    case "instance_file":
                        this.InstanceFile = keyAndValue[1];
                        break;
                    case "test_instance_file":
                        this.TestInstanceFile = keyAndValue[1];
                        break;
                    case "run_obj":
                        this.OptimizeQuality = Equals(keyAndValue[1], "quality");
                        break;
                    case "overall_obj":
                        this.PenalizationFactor = Scenario.ExtractPenalizationFactor(keyAndValue[1]);
                        break;
                    case "paramfile":
                        this.ParameterFile = keyAndValue[1];
                        break;
                    default:
                        continue;
                }
            }

            this.Validate();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the command which starts a target algorithm run.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the cutoff time after which a single target algorithm run is terminated.
        /// </summary>
        public TimeSpan CutoffTime { get; }

        /// <summary>
        /// Gets the path to a file specifying pairs of seeds and training instances.
        /// </summary>
        public string InstanceFile { get; }

        /// <summary>
        /// Gets the path to a file specifying pairs of seeds and test instances.
        /// </summary>
        public string TestInstanceFile { get; }

        /// <summary>
        /// Gets the path to a PCS file specifying the parameters.
        /// </summary>
        public string ParameterFile { get; }

        /// <summary>
        /// Gets a value indicating whether to optimize quality (instead of runtime).
        /// </summary>
        public bool OptimizeQuality { get; } = false;

        /// <summary>
        /// Gets the factor to penalize cancelled runs with in runtime tuning.
        /// </summary>
        public int PenalizationFactor { get; } = 1;

        #endregion

        #region Methods

        /// <summary>
        /// Extracts the penalization factor for cancelled runs.
        /// </summary>
        /// <param name="specification">The specification given for overall objective.</param>
        /// <returns>The penalization factor. 1 if there is none.</returns>
        /// <exception cref="FormatException">Thrown if the specification has a wrong format.</exception>
        private static int ExtractPenalizationFactor(string specification)
        {
            var findPar = new Regex(@"^mean(.+)$");
            if (!findPar.IsMatch(specification))
            {
                return 1;
            }

            var factorAfterMean = findPar.Match(specification).Groups[1].Value;
            if (!int.TryParse(factorAfterMean, out var penalizationFactor))
            {
                throw new FormatException($"Penalization factor {factorAfterMean} should be an integer.");
            }

            return penalizationFactor;
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        private void Validate()
        {
            if (string.IsNullOrEmpty(this.Command))
            {
                throw new ArgumentNullException("algo", "Command should have been set.");
            }

            if (this.CutoffTime == TimeSpan.Zero)
            {
                throw new ArgumentNullException("cutoff_time", "Cutoff time should have been set.");
            }

            if (string.IsNullOrEmpty(this.InstanceFile))
            {
                throw new ArgumentNullException("instance_file", "Training instance file path should have been set.");
            }

            if (string.IsNullOrEmpty(this.ParameterFile))
            {
                throw new ArgumentNullException("paramfile", "Parameter file path should have been set.");
            }
        }

        #endregion
    }
}