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

namespace Optano.Algorithm.Tuner.Application
{
    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;

    /// <summary>
    /// Utility class for parsing command line arguments.
    /// </summary>
    public class ArgumentParser : AdapterArgumentParser<ApplicationRunnerConfiguration.ApplicationConfigurationBuilder>
    {
        #region Methods

        /// <inheritdoc />
        protected override void CheckForRequiredArgumentsAndThrowException()
        {
            if (this.IsMaster)
            {
                if (!this.InternalConfigurationBuilder.HasBasicCommand)
                {
                    throw new OptionException("The basic command for the target algorithm must be provided for master.", "basicCommand");
                }

                if (!this.InternalConfigurationBuilder.HasPathToParameterTree)
                {
                    throw new OptionException("The path to parameter tree must be provided for master.", "parameterTree");
                }
            }
        }

        /// <inheritdoc />
        protected override OptionSet CreateAdapterMasterOptionSet()
        {
            var options = new OptionSet
                              {
                                  {
                                      "basicCommand=", () =>
                                          "The basic {COMMAND} to the target algorithm as it should be executed by the command line. The path to the instance file and the parameters will be set by replacing '{{instance}}' and '{{arguments}}'.",
                                      c => this.InternalConfigurationBuilder.SetBasicCommand(c)
                                  },
                                  {
                                      "parameterTree=", () => "{PATH} to an XML file specifying a parameter tree.",
                                      p => this.InternalConfigurationBuilder.SetPathToParameterTree(p)
                                  },
                                  {
                                      "byValue", () =>
                                          "Indicates that the target algorithm should be tuned by last number in its output instead of by process runtime.\nUsually, you should set --enableRacing=false and refrain from setting --cpuTimeout when using this.",
                                      v => this.InternalConfigurationBuilder.SetTuneByValue(true)
                                  },
                                  {
                                      "ascending=", () =>
                                          "Additional parameter if --byValue is set. Indicates whether low values are better than high ones.\nDefault is true.\nMust be a boolean.",
                                      (bool a) => this.InternalConfigurationBuilder.SetSortValuesAscendingly(a)
                                  },
                                  {
                                      "k|parK=", () =>
                                          "The factor for the penalization of the average runtime. This factor is only applied, if --byValue is not set. Needs to be greater or equal to 0. If 0, OAT sorts first by highest number of uncancelled runs and then by unpenalized average runtime. Default is 0.",
                                      (int factorParK) => this.InternalConfigurationBuilder.SetFactorParK(factorParK)
                                  },
                              };

            return options;
        }

        #endregion
    }
}