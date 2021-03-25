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

namespace Optano.Algorithm.Tuner.Saps
{
    using System;
    using System.Collections.Generic;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;

    /// <summary>
    /// A parser for all SAPS-specific arguments.
    /// </summary>
    public class SapsRunnerConfigurationParser : HelpSupportingArgumentParser<SapsRunnerConfiguration.SapsConfigBuilder>
    {
        #region Fields

        /// <summary>
        /// List of arguments that could not be parsed when calling <see cref="ParseArguments(string[])"/>.
        /// </summary>
        private List<string> _remainingArguments;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SapsRunnerConfigurationParser"/> class.
        /// </summary>
        public SapsRunnerConfigurationParser()
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
                return;
            }

            // If we look at the master, parse remaining arguments.
            if (this.InternalConfigurationBuilder.IsMaster)
            {
                this._remainingArguments = this.CreateMasterOptionSet().Parse(this._remainingArguments);
            }

            this.FinishedParsing();

            // Finally check for required arguments.
            if (this.InternalConfigurationBuilder.IsMaster)
            {
                if (!this.InternalConfigurationBuilder.HasPathToExecutable)
                {
                    throw new OptionException("Path to ubcsat executable must be provided for master.", "executable");
                }
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
                "Additional arguments depending on whether this instance of Optano.Algorithm.Tuner acts as a worker or the master:");
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
                () => "Indicates that this instance of the application should act as master.",
                (string m) => this.InternalConfigurationBuilder.SetIsMaster(true));
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
                                      "executable=",
                                      () => "{PATH} to the ubcsat executable",
                                      p => this.InternalConfigurationBuilder.SetPathToExecutable(p)
                                  },
                                  {
                                      "genericParameterization=",
                                      () =>
                                          "Specifies the generic parameterization to use for the genetic enginering model. Must be a member of the {Optano.Algorithm.Tuner.Saps.CustomModel.GenericParameterization} enum.",
                                      g => this.SetGenericParameterization(g)
                                  },
                                  {
                                      "factorParK=",
                                      () => "The factor for the penalization of the average runtime. Needs to be greater than 0. Default is 10.",
                                      (int f) => this.InternalConfigurationBuilder.SetFactorParK(f)
                                  },
                                  {
                                      "rngSeed=",
                                      () =>
                                          "The random number generator seed, which generates #numberOfSeeds seeds for every instance of the SAPS algorithm. Default is 42.",
                                      (int s) => this.InternalConfigurationBuilder.SetRngSeed(s)
                                  },
                                  {
                                      "numberOfSeeds=",
                                      () =>
                                          "Specifies the number of seeds, which are used for every instance of the SAPS algorithm. Needs to be greater than 0. Default is 1.",
                                      (int n) => this.InternalConfigurationBuilder.SetNumberOfSeeds(n)
                                  },
                              };
            return options;
        }

        /// <summary>
        /// Sets the <see cref="GenericParameterization"/> of the tuner.
        /// </summary>
        /// <param name="genericParam">GenericParameterization to set.</param>
        private void SetGenericParameterization(string genericParam)
        {
            var success = Enum.TryParse(genericParam, true, out GenericParameterization input);
            if (!success)
            {
                throw new OptionException($"Not a member of GenericParameterization: {genericParam}", "genericParameterization");
            }

            this.InternalConfigurationBuilder.SetGenericParameterization(input);
        }

        #endregion
    }
}