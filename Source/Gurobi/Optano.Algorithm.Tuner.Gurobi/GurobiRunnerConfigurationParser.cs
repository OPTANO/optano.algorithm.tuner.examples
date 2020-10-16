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

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;

    /// <summary>
    /// A parser for console arguments to start CEP2NET tuning.
    /// </summary>
    public class GurobiRunnerConfigurationParser : HelpSupportingArgumentParser<GurobiRunnerConfiguration.GurobiRunnerConfigBuilder>
    {
        #region Fields

        /// <summary>
        /// List of arguments that could not be parsed when calling <see cref="ParseArguments(string[])"/>.
        /// </summary>
        private List<string> _remainingArguments;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunnerConfigurationParser"/> class.
        /// </summary>
        public GurobiRunnerConfigurationParser()
            : base(allowAdditionalArguments: true)
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the arguments that could not be parsed when calling <see cref="ParseArguments(string[])"/>.
        /// </summary>
        public List<string> RemainingArguments
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._remainingArguments;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance of OPTANO Algorithm Tuner should act as worker or master.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if called before <see cref="ParseArguments(string[])"/> has been executed.
        /// </exception>
        public bool MasterRequested
        {
            get
            {
                return this.ConfigurationBuilder.IsMaster;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Parses the provided arguments.
        /// </summary>
        /// <param name="args">Arguments to parse.</param>
        /// <exception cref="OptionException">Thrown if required parameters have not been set.</exception>
        public override void ParseArguments(string[] args)
        {
            // First check for options influencing which other options are possible.
            this._remainingArguments = this.CreatePreprocessingOptionSet().Parse(args);
            this.FinishedPreProcessing();

            // Don't verify further if help was requested.
            if (this.HelpTextRequested)
            {
                this.PrintHelp();
                return;
            }

            // If we look at the master, parse remaining arguments.
            if (this.InternalConfigurationBuilder.IsMaster)
            {
                this._remainingArguments = this.CreateMasterOptionSet().Parse(this._remainingArguments);
            }

            this.FinishedParsing();

            // Finally check for potential required arguments.
            if (this.MasterRequested)
            {
                // Throw an exception here, if a required argument exists and was not given.
            }
        }

        /// <summary>
        /// Prints a description on how to use command line arguments.
        /// </summary>
        public override void PrintHelp()
        {
            // Print own arguments.
            Console.Out.WriteLine("Arguments for the application:");
            this.CreatePreprocessingOptionSet().WriteOptionDescriptions(Console.Out);
            Console.Out.WriteLine("\nAdditional required arguments if this instance acts as master (i.e. --master provided):");
            this.CreateMasterOptionSet().WriteOptionDescriptions(Console.Out);
            Console.Out.WriteLine();

            // Print arguments for master and worker.
            Console.Out.WriteLine(
                "Additional arguments depending on whether this instance of OPTANO Algorithm Tuner acts as a worker or the master:");
            Console.Out.WriteLine();
            new MasterArgumentParser().PrintHelp(false);
            Console.Out.WriteLine();
            new WorkerArgumentParser().PrintHelp(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an <see cref="OptionSet"/> containing all options that somehow influence which other options can be
        /// set.
        /// </summary>
        /// <returns>The created <see cref="OptionSet"/>.</returns>
        private OptionSet CreatePreprocessingOptionSet()
        {
            var options = this.CreateOptionSet();
            options.Add(
                "master",
                "Indicates that this instance of the application should act as master.",
                m => this.InternalConfigurationBuilder.SetIsMaster(true));
            return options;
        }

        /// <summary>
        /// Creates an <see cref="OptionSet"/> containing all options that can only be set if the "--master" option is set.
        /// </summary>
        /// <returns>The created <see cref="OptionSet"/>.</returns>
        private OptionSet CreateMasterOptionSet()
        {
            var options = new OptionSet
                              {
                                  {
                                      "grbThreadCount=",
                                      "Sets the maximum number of threads that may be used by Gurobi.\r\nDefault is 4. Needs to be greater than 0.",
                                      (int t) => this.InternalConfigurationBuilder.SetThreadCount(t)
                                  },
                                  {
                                      "numberOfSeeds=",
                                      "Sets the number of random seeds to use for every .mps file found in the instance folder.\r\nFor each file, numberOfSeeds many independent seeds will be used,\r\neffectively increasing the instance count by a factor of numberOfSeeds.\r\nDefault is 1. Needs to be greater than 0.",
                                      (int n) => this.InternalConfigurationBuilder.SetNumberOfSeeds(n)
                                  },
                                  {
                                      "rngSeed=",
                                      "Sets the random number generator seed, which generates #numberOfSeeds seeds for every instance of the Gurobi algorithm. Default is 42.",
                                      (int s) => this.InternalConfigurationBuilder.SetRngSeed(s)
                                  },
                                  {
                                      "grbNodefileDirectory=",
                                      "Sets the nodefile directory of Gurobi. Default is a subfolder 'nodefiles' in the current working directory.",
                                      (string nd) => this.InternalConfigurationBuilder.SetNodefileDirectory(nd)
                                  },
                                  {
                                      "grbNodefileStartSizeGigabyte=",
                                      "Sets the memory threshold in gigabyte of Gurobi for writing MIP tree nodes in nodefile on disk. Default is 0.5 GB. Needs to be greater than or equal to 0.",
                                      (double nss) => this.InternalConfigurationBuilder.SetNodefileStartSizeGigabyte(nss)
                                  },
                                  {
                                      "grbTerminationMipGap=",
                                      "Sets the termination mip gap of Gurobi. Default is 0.01. Needs to be greater than or equal to 0.",
                                      (double mg) => this.InternalConfigurationBuilder.SetTerminationMipGap(mg)
                                  },
                              };
            return options;
        }

        #endregion
    }
}