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
    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;

    /// <summary>
    /// A parser for all ACLib-specific arguments.
    /// </summary>
    public class ArgumentParser : AdapterArgumentParser<AcLibRunnerConfiguration.AcLibRunnerConfigurationBuilder>
    {
        #region Methods

        /// <inheritdoc />
        protected override void CheckForRequiredArgumentsAndThrowException()
        {
            if (this.IsMaster && !this.InternalConfigurationBuilder.HasPathToScenarioFile)
            {
                throw new OptionException("The path to scenario file must be provided for master.", "scenario");
            }
        }

        /// <inheritdoc />
        protected override OptionSet CreateAdapterMasterOptionSet()
        {
            var options = new OptionSet
                              {
                                  {
                                      "scenario=",
                                      () => "Sets the {PATH} to a text file specifying the scenario. The format of this file is the one by ACLib.",
                                      path => this.InternalConfigurationBuilder.SetPathToScenarioFile(path)
                                  },
                              };
            return options;
        }

        #endregion
    }
}