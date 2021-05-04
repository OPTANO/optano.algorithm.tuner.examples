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
    using System;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.AcLib.Configuration;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="ArgumentParser"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class ArgumentParserTest : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="ArgumentParser"/> used in tests. Must be initialized.
        /// </summary>
        private readonly ArgumentParser _parser;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParserTest"/> class.
        /// </summary>
        public ArgumentParserTest()
        {
            this._parser = new ArgumentParser();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Checks that all options are parsed correctly.
        /// </summary>
        [Fact]
        public void OptionsAreParsedCorrectly()
        {
            string[] args =
                {
                    "--master",
                    "--scenario=testScenario",
                };
            this._parser.ParseArguments(args);
            var config = this._parser.ConfigurationBuilder.Build();

            Assert.True(this._parser.IsMaster);
            Assert.Equal("testScenario", config.PathToScenarioFile);
        }

        /// <summary>
        /// Verifies that parsing without providing
        /// a path to a scenario file throws an <see cref="OptionException"/> if --master is set.
        /// </summary>
        [Fact]
        public void NoPathToScenarioFileThrowsExceptionForMaster()
        {
            Assert.Throws<OptionException>(() => this._parser.ParseArguments(new string[] { "--master" }));
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        #endregion
    }
}