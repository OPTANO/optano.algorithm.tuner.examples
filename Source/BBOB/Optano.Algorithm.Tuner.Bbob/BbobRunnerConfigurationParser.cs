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

namespace Optano.Algorithm.Tuner.Bbob
{
    using System;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;

    /// <summary>
    /// A parser for all BBOB-specific arguments.
    /// </summary>
    /// <seealso cref="BbobRunnerConfiguration.BbobRunnerConfigurationBuilder" />
    public class BbobRunnerConfigurationParser : AdapterArgumentParser<BbobRunnerConfiguration.BbobRunnerConfigurationBuilder>
    {
        #region Static Fields

        /// <summary>
        /// The name of the option, which sets the python binary.
        /// </summary>
        public static readonly string PythonBinName = "pythonBinary";

        /// <summary>
        /// The name of the option, which sets the BBOB function id.
        /// </summary>
        public static readonly string FunctionIdName = "functionId";

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void CheckForRequiredArgumentsAndThrowException()
        {
            if (this.IsMaster)
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

        /// <inheritdoc />
        protected override OptionSet CreateAdapterMasterOptionSet()
        {
            var options = new OptionSet
                              {
                                  {
                                      "instanceSeed=", () =>
                                          $"The random seed for the instance seed generator. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.InstanceSeedDefault}.",
                                      (int s) => this.InternalConfigurationBuilder.SetInstanceSeed(s)
                                  },
                                  {
                                      BbobRunnerConfigurationParser.PythonBinName + "=", () => "The path to the python 2.7 binary.",
                                      p => this.InternalConfigurationBuilder.SetPythonBin(p)
                                  },
                                  {
                                      "bbobScript=", () =>
                                          $"The path to the BBOB python 2.7 adapter script. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.PathToExecutableDefault}.",
                                      b => this.InternalConfigurationBuilder.SetPathToExecutable(b)
                                  },
                                  {
                                      BbobRunnerConfigurationParser.FunctionIdName + "=",
                                      () => "The bbob function to use. Must be in the range [1,56].",
                                      (int f) => this.InternalConfigurationBuilder.SetFunctionId(f)
                                  },
                                  {
                                      "dimensions=", () =>
                                          $"The number of dimensions for the BBOB function. Must be greater than 0. Default is {BbobRunnerConfiguration.BbobRunnerConfigurationBuilder.DimensionsDefault}.",
                                      (int d) => this.InternalConfigurationBuilder.SetDimensions(d)
                                  },
                                  {
                                      "genericParameterization=",
                                      () =>
                                          "Specifies the generic parameterization to use for the genetic engineering model. Must be a member of the GenericParameterization enum.",
                                      (string genericParamString) =>
                                          {
                                              if (!Enum.TryParse(genericParamString, true, out GenericParameterization genericParam))
                                              {
                                                  throw new OptionException(
                                                      "The given generic parameterization is not a member of the GenericParameterization enum.",
                                                      "genericParameterization");
                                              }

                                              this.InternalConfigurationBuilder.SetGenericParameterization(genericParam);
                                          }
                                  },
                              };
            return options;
        }

        #endregion
    }
}