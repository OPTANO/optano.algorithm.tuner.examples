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
    using Akka.Util.Internal;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.GrayBox.PostTuningRunner;

    /// <summary>
    /// A parser for console arguments to start Gurobi tuning.
    /// </summary>
    public class GurobiRunnerConfigurationParser : PostTuningAdapterArgumentParser<GurobiRunnerConfiguration.GurobiRunnerConfigBuilder>
    {
        #region Methods

        /// <inheritdoc />
        protected override OptionSet CreateAdapterMasterOptionSet()
        {
            var options = this.CreateMasterOrPostTuningOptionSet();

            var additionalMasterOptions = new OptionSet
                                              {
                                                  {
                                                      "numberOfSeeds=",
                                                      () =>
                                                          "Sets the number of random seeds to use for every .mps file found in the instance folder.\r\nFor each file, numberOfSeeds many independent seeds will be used,\r\neffectively increasing the instance count by a factor of numberOfSeeds.\r\nDefault is 1. Needs to be greater than 0.",
                                                      (int n) => this.InternalConfigurationBuilder.SetNumberOfSeeds(n)
                                                  },
                                                  {
                                                      "rngSeed=",
                                                      () =>
                                                          "Sets the random number generator seed, which generates #numberOfSeeds seeds for every instance of the Gurobi algorithm. Default is 42.",
                                                      (int s) => this.InternalConfigurationBuilder.SetRngSeed(s)
                                                  },
                                              };
            additionalMasterOptions.ForEach(option => options.Add(option));

            return options;
        }

        /// <inheritdoc />
        protected override OptionSet CreateAdapterPostTuningOptionSet()
        {
            return this.CreateMasterOrPostTuningOptionSet();
        }

        /// <summary>
        /// Creates an <see cref="OptionSet"/> containing all options that can be set if the master or a post tuning runner is requested.
        /// </summary>
        /// <returns>The created <see cref="OptionSet"/>.</returns>
        private OptionSet CreateMasterOrPostTuningOptionSet()
        {
            var options = new OptionSet
                              {
                                  {
                                      "grbThreadCount=",
                                      () =>
                                          "Sets the maximum number of threads that may be used by Gurobi.\r\nDefault is 4. Needs to be greater than 0.",
                                      (int t) => this.InternalConfigurationBuilder.SetThreadCount(t)
                                  },
                                  {
                                      "grbNodefileDirectory=",
                                      () =>
                                          "Sets the nodefile directory of Gurobi. Default is a subfolder 'nodefiles' in the current working directory.",
                                      (string nd) => this.InternalConfigurationBuilder.SetNodefileDirectory(nd)
                                  },
                                  {
                                      "grbNodefileStartSizeGigabyte=",
                                      () =>
                                          "Sets the memory threshold in gigabyte of Gurobi for writing MIP tree nodes in nodefile on disk. Default is 0.5 GB. Needs to be greater than or equal to 0.",
                                      (double nss) => this.InternalConfigurationBuilder.SetNodefileStartSizeGigabyte(nss)
                                  },
                                  {
                                      "grbTerminationMipGap=",
                                      () => "Sets the termination mip gap of Gurobi. Default is 0.01. Needs to be greater than or equal to 0.",
                                      (double mg) => this.InternalConfigurationBuilder.SetTerminationMipGap(mg)
                                  },
                              };

            return options;
        }

        #endregion
    }
}