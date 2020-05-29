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

namespace Optano.Algorithm.Tuner.Application.Tests
{
    using System;
    using System.IO;
    using System.Linq;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ArgumentParser"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ArgumentParserTest
    {
        #region Fields

        /// <summary>
        /// The <see cref="ArgumentParser"/> used in tests. Must be initialized.
        /// </summary>
        private readonly ArgumentParser _parser;

        #endregion

        #region Constructors and Destructors

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParserTest"/> class.
        /// </summary>
        public ArgumentParserTest()
        {
            TestUtils.InitializeLogger();
            this._parser = new ArgumentParser();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Verifies that accessing <see cref="HelpSupportingArgumentParserBase.HelpTextRequested"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void AccessingHelpTextRequestedBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.HelpTextRequested);
        }

        /// <summary>
        /// Verifies that accessing <see cref="HelpSupportingArgumentParserBase.AdditionalArguments"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void AccessingAdditionalArgumentsBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.AdditionalArguments);
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> with --help
        /// does not throw an exception even if invalid arguments are given.
        /// </summary>
        [Fact]
        public void HelpLongOptionPreventsExceptions()
        {
            string[] args = new string[]
                                {
                                    "--help",
                                    "--invaliiiiid",
                                };
            this._parser.ParseArguments(args);
            Assert.True(this._parser.HelpTextRequested, "Help text should have been requested.");
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> with -h does
        /// not throw an exception even if invalid arguments are given.
        /// </summary>
        [Fact]
        public void HelpShortOptionPreventsExceptions()
        {
            string[] args = new string[]
                                {
                                    "-h",
                                    "--invaliiiiid",
                                };
            this._parser.ParseArguments(args);
            Assert.True(this._parser.HelpTextRequested, "Help text should have been requested.");
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Verifies that accessing <see cref="ArgumentParser.BasicCommand"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void AccessingBasicCommandBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.BasicCommand);
        }

        /// <summary>
        /// Verifies that accessing <see cref="ArgumentParser.PathToParameterTree"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void AccessingPathToParameterTreeeBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.PathToParameterTree);
        }

        /// <summary>
        /// Verifies that accessing <see cref="ArgumentParser.MasterRequested"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void AccessingMasterRequestedBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.MasterRequested);
        }

        /// <summary>
        /// Verifies that accessing <see cref="ArgumentParser.TuneByValue"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void TuneByValueRequestedBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.TuneByValue);
        }

        /// <summary>
        /// Verifies that accessing <see cref="ArgumentParser.SortValuesAscendingly"/> before calling
        /// <see cref="ArgumentParser.ParseArguments(string[])"/> throws an
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [Fact]
        public void SortValuesAscendinglyRequestedBeforeParsingThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => this._parser.SortValuesAscendingly);
        }

        /// <summary>
        /// Checks that <see cref="ArgumentParser.ParseArguments(string[])"/> correctly interprets arguments.
        /// </summary>
        [Fact]
        public void OptionsAreParsedCorrectly()
        {
            string[] args = new string[]
                                {
                                    "--basicCommand=multiple words",
                                    "--parameterTree=testTree",
                                    "--master",
                                    "--byValue",
                                    "--ascending=false",
                                };

            this._parser.ParseArguments(args);
            Assert.Equal("testTree", this._parser.PathToParameterTree);
            Assert.Equal("multiple words", this._parser.BasicCommand);
            Assert.True(this._parser.MasterRequested, "Master option was not parsed correctly.");
            Assert.True(this._parser.TuneByValue, "Tuning by value option was not parsed correctly.");
            Assert.False(this._parser.SortValuesAscendingly, "Sort direction was not parsed correctly.");
        }

        /// <summary>
        /// Verifies that calling <see cref="ArgumentParser.ParseArguments(string[])"/> without providing
        /// a basic command throws an <see cref="OptionException"/> if --master is set.
        /// </summary>
        [Fact]
        public void NoBasicCommandThrowsExceptionForMaster()
        {
            Assert.Throws<OptionException>(() => this._parser.ParseArguments(new string[] { "--master", "--parameterTree=testTree" }));
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> without providing 
        /// a basic command does not throw an exception if --master is not set.
        /// </summary>
        [Fact]
        public void NoBasicCommandDoesNotThrowForWorker()
        {
            this._parser.ParseArguments(new string[] { "--parameterTree=testTree" });
            Assert.Null(this._parser.BasicCommand);
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> and providing a 
        /// basic command, but not setting --master, prints a warning to the console.
        /// </summary>
        [Fact]
        public void BasicCommandPrintsWarningForWorker()
        {
            TestUtils.CheckOutput(
                action: () => this._parser.ParseArguments(new string[] { "--basicCommand=testCmd" }),
                check: consoleOutput =>
                    {
                        using (StringReader reader = new StringReader(consoleOutput.ToString()))
                        {
                            Assert.True(
                                reader.ReadLine().Contains("WARNING"),
                                "Basic command was specified for worker, but no warning was printed.");
                        }
                    });
        }

        /// <summary>
        /// Verifies that calling <see cref="ArgumentParser.ParseArguments(string[])"/> without providing
        /// a path to a parameter tree throws an <see cref="OptionException"/> if --master is set.
        /// </summary>
        [Fact]
        public void NoPathToParameterTreeThrowsExceptionForMaster()
        {
            Assert.Throws<OptionException>(() => this._parser.ParseArguments(new string[] { "--master", "--basicCommand=testCmd" }));
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> without providing a
        /// path to a parameter tree does not throw an exception if --master is not set.
        /// </summary>
        [Fact]
        public void NoPathToParameterTreeDoesNotThrowForWorker()
        {
            this._parser.ParseArguments(new string[] { "--basicCommand=testCmd" });
            Assert.Null(this._parser.PathToParameterTree);
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> and providing a 
        /// parameter tree, but not setting --master, prints a warning to the console.
        /// </summary>
        [Fact]
        public void PathToParameterTreePrintsWarningForWorker()
        {
            TestUtils.CheckOutput(
                action: () => this._parser.ParseArguments(new string[] { "--parameterTree=testTree" }),
                check: consoleOutput =>
                    {
                        using (StringReader reader = new StringReader(consoleOutput.ToString()))
                        {
                            Assert.True(
                                reader.ReadLine().Contains("WARNING"),
                                "Parameter tree was specified for worker, but no warning was printed.");
                        }
                    });
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> and providing the --byValue
        /// parameter, but not setting --master, prints a warning to the console.
        /// </summary>
        [Fact]
        public void TuneByValuePrintsWarningForWorker()
        {
            TestUtils.CheckOutput(
                action: () => this._parser.ParseArguments(new string[] { "--byValue" }),
                check: consoleOutput =>
                    {
                        using (StringReader reader = new StringReader(consoleOutput.ToString()))
                        {
                            Assert.True(
                                reader.ReadLine().Contains("WARNING"),
                                "Kind of tuning was specified for worker, but no warning was printed.");
                        }
                    });
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> and providing the sort direction,
        /// but not setting --master, prints a warning to the console.
        /// </summary>
        [Fact]
        public void SortDirectionPrintsWarningForWorker()
        {
            TestUtils.CheckOutput(
                action: () => this._parser.ParseArguments(new string[] { "--ascending=false" }),
                check: consoleOutput =>
                    {
                        using (StringReader reader = new StringReader(consoleOutput.ToString()))
                        {
                            Assert.True(
                                reader.ReadLine().Contains("WARNING"),
                                "Sort direction was specified for worker, but no warning was printed.");
                        }
                    });
        }

        /// <summary>
        /// Checks that calling <see cref="ArgumentParser.ParseArguments(string[])"/> and providing the sort direction,
        /// but not setting --byValue, prints a warning to the console when --master is provided.
        /// </summary>
        [Fact]
        public void SortDirectionWithoutTuneByValuePrintsWarningForMaster()
        {
            var args = new string[]
                           {
                               "--master",
                               "--ascending=false",
                               "--basicCommand=multiple words",
                               "--parameterTree=testTree",
                           };
            TestUtils.CheckOutput(
                action: () => this._parser.ParseArguments(args),
                check: consoleOutput =>
                    {
                        using (StringReader reader = new StringReader(consoleOutput.ToString()))
                        {
                            Assert.True(
                                reader.ReadLine().Contains("WARNING"),
                                "Sort direction without value tuning specified for master, but no warning was printed.");
                        }
                    });
        }

        /// <summary>
        /// Checks that a call to <see cref="ArgumentParser.PrintHelp"/> prints all arguments, including those for both
        /// master and workers.
        /// </summary>
        [Fact]
        public void PrintHelpPrintsAllArguments()
        {
            TestUtils.CheckOutput(
                action: () => this._parser.PrintHelp(),
                check: consoleOutput =>
                    {
                        StringReader reader = new StringReader(consoleOutput.ToString());
                        var text = reader.ReadToEnd();
                        Assert.True(
                            text.Contains("Arguments for the application:"),
                            "Application arguments are missing.");
                        Assert.True(text.Contains("Arguments for master:"), "Master arguments are missing.");
                        Assert.True(text.Contains("Arguments for worker:"), "Worker arguments are missing.");
                    });
        }

        /// <summary>
        /// Checks that arguments that could not be parsed will be returned upon calling
        /// <see cref="HelpSupportingArgumentParserBase.AdditionalArguments"/>.
        /// </summary>
        [Fact]
        public void AdditionalArgumentsAreStored()
        {
            string[] args = new string[]
                                {
                                    "--basicCommand=multiple words",
                                    "--parameterTree=testTree",
                                    "--master",
                                    "--halp",
                                };
            this._parser.ParseArguments(args);
            Assert.Equal("--halp", this._parser.AdditionalArguments.Single());
        }

        #endregion
    }
}
