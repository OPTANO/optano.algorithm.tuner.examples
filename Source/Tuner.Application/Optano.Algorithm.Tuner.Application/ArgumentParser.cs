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

namespace Optano.Algorithm.Tuner.Application
{
    using System;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;
    using Optano.Algorithm.Tuner.DistributedExecution;
    using Optano.Algorithm.Tuner.Logging;

    /// <summary>
    /// Utility class for parsing command line arguments.
    /// </summary>
    public class ArgumentParser : HelpSupportingArgumentParserBase
    {
        #region Fields

        /// <summary>
        /// The basic command to the target algorithm as specified by the parsed arguments.
        /// </summary>
        private string _basicCommand;

        /// <summary>
        /// The path to an XML file defining the parameter tree as specified by the parsed arguments.
        /// </summary>
        private string _pathToParameterTree;

        /// <summary>
        /// A value indicating whether the target algorithm should be tuned by value (otherwise, it's tuned by
        /// runtime).
        /// </summary>
        private bool _tuneByValue = false;

        /// <summary>
        /// A value indicating whether lower values are better than higher ones.
        /// </summary>
        private bool _sortValuesAscendingly = true;

        /// <summary>
        /// A value indicating whether the instance of OPTANO Algorithm Tuner should act as worker or master.
        /// </summary>
        private bool _isMaster = false;

        /// <summary>
        /// PAR k Factor.
        /// </summary>
        private int _factorPar = 1;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser"/> class.
        /// </summary>
        public ArgumentParser()
            : base(allowAdditionalArguments: true)
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the basic command to the target algorithm.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called before <see cref="ParseArguments(string[])"/>
        /// has been executed.</exception>
        public string BasicCommand
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._basicCommand;
            }
        }

        /// <summary>
        /// Gets the path to an XML file defining the parameter tree.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called before <see cref="ParseArguments(string[])"/>
        /// has been executed.</exception>
        public string PathToParameterTree
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._pathToParameterTree;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance of OPTANO Algorithm Tuner should act as worker or master.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called before <see cref="ParseArguments(string[])"/>
        /// has been executed.</exception>
        public bool MasterRequested
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._isMaster;
            }
        }

        /// <summary>
        /// Gets a value indicating whether whether the target algorithm should be tuned by value (otherwise, it's
        /// tuned by runtime).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called before <see cref="ParseArguments(string[])"/>
        /// has been executed.</exception>
        public bool TuneByValue
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._tuneByValue;
            }
        }

        /// <summary>
        /// Gets a value indicating whether whether lower values are better than higher ones.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if called before <see cref="ParseArguments(string[])"/>
        /// has been executed.</exception>
        public bool SortValuesAscendingly
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._sortValuesAscendingly;
            }
        }

        /// <summary>
        /// Gets the factor to penalize timed out runs with.
        /// Default: 1.
        /// </summary>
        public int FactorPar
        {
            get
            {
                this.ThrowExceptionIfNoParsingHasBeenDone();
                return this._factorPar;
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
            base.ParseArguments(args);

            // Don't do further verification if help was requested.
            if (this.HelpTextRequested)
            {
                return;
            }

            // If no help was requested...
            if (this.MasterRequested)
            {
                // ...and --master was provided, first check for required arguments.
                if (this._basicCommand == null)
                {
                    throw new OptionException("Basic command for target algorithm must be provided for master.", "basicCommand");
                }

                if (this._pathToParameterTree == null)
                {
                    throw new OptionException("Path to parameter tree must be provided for master.", "parameterTree");
                }

                // Then print a warning for changed sorting if tune by value is missing.
                if (!this._sortValuesAscendingly && !this._tuneByValue)
                {
                    LoggingHelper.WriteLine(
                        VerbosityLevel.Warn,
                        "WARNING: Sort direction is ignored when not sorting by value.");
                }

                if (this._tuneByValue && this._factorPar != 1)
                {
                    LoggingHelper.WriteLine(
                        VerbosityLevel.Warn,
                        "WARNING: FactorPar will be ignored while byValue==true");
                }
            }
            else
            {
                // If the worker is configured instead, print warnings for useless parameters.
                if (this._basicCommand != null)
                {
                    LoggingHelper.WriteLine(
                        VerbosityLevel.Warn,
                        "WARNING: Basic command argument is ignored for workers.");
                }

                if (this._pathToParameterTree != null)
                {
                    LoggingHelper.WriteLine(
                        VerbosityLevel.Warn,
                        "WARNING: Path to parameter tree argument is ignored for workers.");
                }

                if (this._tuneByValue)
                {
                    LoggingHelper.WriteLine(
                        VerbosityLevel.Warn,
                        "WARNING: Kind of tuning argument is ignored for workers.");
                }

                // sortValuesAscendingly has to be checked the other way around because the default is true.
                if (!this._sortValuesAscendingly)
                {
                    LoggingHelper.WriteLine(
                        VerbosityLevel.Warn,
                        "WARNING: Sort direction argument is ignored for workers.");
                }
            }
        }

        /// <summary>
        /// Prints a description on how to use the command line arguments.
        /// </summary>
        public override void PrintHelp()
        {
            // Print own arguments.
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
        /// Creates an <see cref="OptionSet" /> containing all options important for running
        /// Optano.Algorithm.Tuner.Application.exe.
        /// </summary>
        /// <returns>The created <see cref="OptionSet" />.</returns>
        protected override OptionSet CreateOptionSet()
        {
            var options = base.CreateOptionSet();
            options.Add("master", "Indicates that this instance of the application should act as master.", m => this._isMaster = true);
            options.Add(
                "basicCommand=",
                "The basic {COMMAND} to the target algorithm as it should be executed by the command line. The path to the instance file and the parameters will be set by replacing '{{instance}}' and '{{arguments}}'.\nUse iff --master is used.",
                c => this._basicCommand = c);
            options.Add(
                "parameterTree=",
                "{PATH} to an XML file specifying a paramter tree.\nUse iff --master is used.",
                p => this._pathToParameterTree = p);
            options.Add(
                "byValue",
                "Indicates that the target algorithm should be tuned by last number in its output instead of by process runtime.\nUsually, you should set --enableRacing=false and refrain from setting --cpuTimeout when using this.",
                v => this._tuneByValue = true);
            options.Add(
                "ascending=",
                "Additional parameter if --byValue is set. Indicates whether low values are better than high ones.\nDefault is true.\nMust be a boolean.",
                (bool a) => this._sortValuesAscendingly = a);
            options.Add(
                "k|parK=",
                "The penalization factor for timed out evaluations. \nDefault is 1.\nThis must be an integer greater or equal than 1. Factor is only applied when \"byValue\" flag is not set.",
                (int k) => this._factorPar = k);

            return options;
        }

        #endregion
    }
}