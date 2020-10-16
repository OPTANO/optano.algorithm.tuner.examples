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

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;

    /// <summary>
    /// A parser for all BBOB-specific arguments.
    /// </summary>
    /// <seealso cref="BbobRunnerConfiguration.BbobRunnerConfigurationBuilder" />
    public class BbobRunnerConfigurationParser : HelpSupportingArgumentParser<BbobRunnerConfiguration.BbobRunnerConfigurationBuilder>
    {
        #region Static Fields

        /// <summary>
        /// The name of the option, which sets the python binary.
        /// </summary>
        public static readonly string PythonBinName = "pythonBin";

        /// <summary>
        /// The name of the option, which sets the BBOB function id.
        /// </summary>
        public static readonly string FunctionIdName = "functionId";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BbobRunnerConfigurationParser"/> class.
        /// </summary>
        public BbobRunnerConfigurationParser()
            : base(allowAdditionalArguments: true)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Parses the provided arguments.
        /// </summary>
        /// <param name="args">Arguments to parse.</param>
        /// <exception cref="OptionException">Throws an exception, if <see cref="BbobRunnerConfigurationParser.PythonBinName"/> or <see cref="BbobRunnerConfigurationParser.FunctionIdName"/> is not provided for master.
        /// </exception>
        public override void ParseArguments(string[] args)
        {
            base.ParseArguments(args);

            if (this.InternalConfigurationBuilder.IsMaster)
            {
                if (!this.InternalConfigurationBuilder.HasPythonBin)
                {
                    throw new OptionException(
                        $"{BbobRunnerConfigurationParser.PythonBinName} must be provided for master.",
                        $"{BbobRunnerConfigurationParser.PythonBinName}");
                }

                if (!this.InternalConfigurationBuilder.HasFunctionId)
                {
                    throw new OptionException(
                        $"{BbobRunnerConfigurationParser.FunctionIdName} must be provided for master.",
                        $"{BbobRunnerConfigurationParser.FunctionIdName}");
                }
            }
        }

        /// <summary>
        /// Prints a description on how to use the command line arguments.
        /// </summary>
        public override void PrintHelp()
        {
            Console.Out.WriteLine("Arguments for the application:");
            base.PrintHelp();
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
        /// Creates an <see cref="T:NDesk.Options.OptionSet" /> containing all options this parser can handle.
        /// </summary>
        /// <returns>
        /// The created <see cref="T:NDesk.Options.OptionSet" />.
        /// </returns>
        protected override OptionSet CreateOptionSet()
        {
            var options = base.CreateOptionSet();
            options.Add(
                "master",
                "Indicates that this instance of the application should act as master.",
                (string m) => this.InternalConfigurationBuilder.SetIsMaster(true));
            options.Add(
                "instanceSeed=",
                $"The random seed for the instance seed generator. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.InstanceSeedDefault}.",
                (int s) => this.InternalConfigurationBuilder.SetInstanceSeed(s));
            options.Add(
                BbobRunnerConfigurationParser.PythonBinName + "=",
                "The path to the python 2.7 binary.",
                p => this.InternalConfigurationBuilder.SetPythonBin(p));
            options.Add(
                "bbobScript=",
                $"The path to the BBOB python 2.7 script. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.PathToExecutableDefault}.",
                b => this.InternalConfigurationBuilder.SetPathToExecutable(b));
            options.Add(
                BbobRunnerConfigurationParser.FunctionIdName + "=",
                "The bbob function to use. Must be in the range [1,56].",
                (int f) => this.InternalConfigurationBuilder.SetFunctionId(f));
            options.Add(
                "dimensions=",
                $"The number of dimensions for the BBOB function. Must be greater than 0. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.DimensionsDefault}.",
                (int d) => this.InternalConfigurationBuilder.SetDimensions(d));
            options.Add(
                "genericParameterization=",
                $"Specifies the generic parameterization to use for the genetic enginering model. Must be a member of the GenericParameterization enum. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.GenericParameterizationDefault}.",
                (GenericParameterization g) => this.InternalConfigurationBuilder.SetGenericParameterization(g));
            return options;
        }

        #endregion
    }
}