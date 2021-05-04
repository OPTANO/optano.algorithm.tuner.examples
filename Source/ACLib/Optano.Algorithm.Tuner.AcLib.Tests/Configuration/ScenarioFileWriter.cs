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

namespace Optano.Algorithm.Tuner.AcLib.Tests.Configuration
{
    using System.IO;

    /// <summary>
    /// A simple class to write ACLib scenario files.
    /// </summary>
    public class ScenarioFileWriter
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the run objective.
        /// </summary>
        public string RunObjective { get; set; } = "runtime";

        /// <summary>
        /// Gets or sets the overall objective.
        /// </summary>
        public string OverallObjective { get; set; } = "mean10";

        /// <summary>
        /// Gets or sets the cutoff time.
        /// </summary>
        public string CutoffTime { get; set; } = "300";

        /// <summary>
        /// Gets or sets the training instance file.
        /// </summary>
        public string InstanceFile { get; set; } = "./instances/sat/sets/training.txt";

        /// <summary>
        /// Gets or sets the test instance file.
        /// </summary>
        public string TestInstanceFile { get; set; } = "./instances/sat/sets/test.txt";

        /// <summary>
        /// Gets or sets the parameter file.
        /// </summary>
        public string ParameterFile { get; set; } = "./target_algorithms/sat/probSAT/probSAT_params.pcs";

        /// <summary>
        /// Gets or sets the target algorithm command.
        /// </summary>
        public string Command { get; set; } =
            "python -u ./target_algorithms/sat/scripts/wrapper.py --sat-checker ./target_algorithms/sat/scripts/SAT --sol-file ./instances/sat/true_solubility.txt";

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Writes the scenario to file.
        /// </summary>
        /// <param name="path">The path to write it to.</param>
        public void Write(string path)
        {
            using (var file = File.CreateText(path))
            {
                // Add a line that is ignored by OPTANO Algorithm Tuner.
                file.WriteLine("deterministic = 0");

                if (this.RunObjective != null)
                {
                    file.WriteLine($"run_obj = {this.RunObjective}");
                }

                if (this.OverallObjective != null)
                {
                    file.WriteLine($"overall_obj = {this.OverallObjective}");
                }

                if (this.CutoffTime != null)
                {
                    file.WriteLine($"cutoff_time = {this.CutoffTime}");
                }

                if (this.InstanceFile != null)
                {
                    file.WriteLine($"instance_file = {this.InstanceFile}");
                }

                if (this.TestInstanceFile != null)
                {
                    file.WriteLine($"test_instance_file = {this.TestInstanceFile}");
                }

                if (this.ParameterFile != null)
                {
                    file.WriteLine($"paramfile = {this.ParameterFile}");
                }

                if (this.Command != null)
                {
                    file.WriteLine($"algo = {this.Command}");
                }
            }
        }

        #endregion
    }
}