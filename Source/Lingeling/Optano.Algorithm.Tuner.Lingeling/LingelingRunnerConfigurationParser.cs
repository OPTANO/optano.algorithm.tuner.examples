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

namespace Optano.Algorithm.Tuner.Lingeling
{
    using System;

    using NDesk.Options;

    using Optano.Algorithm.Tuner.Configuration.ArgumentParsers;

    /// <summary>
    /// A parser for all Lingeling-specific arguments.
    /// </summary>
    public class LingelingRunnerConfigurationParser : AdapterArgumentParser<LingelingRunnerConfiguration.LingelingConfigBuilder>
    {
        #region Methods

        /// <inheritdoc />
        protected override void CheckForRequiredArgumentsAndThrowException()
        {
            if (this.IsMaster)
            {
                if (!this.InternalConfigurationBuilder.HasPathToExecutable)
                {
                    throw new OptionException("Path to executable must be provided for master.", "executable");
                }
            }
        }

        /// <inheritdoc />
        protected override OptionSet CreateAdapterMasterOptionSet()
        {
            var options = new OptionSet
                              {
                                  {
                                      "executable=",
                                      () => "{PATH} to the executable",
                                      p => this.InternalConfigurationBuilder.SetPathToExecutable(p)
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
                                  {
                                      "factorParK=",
                                      () =>
                                          "The factor for the penalization of the average runtime. Needs to be greater or equal to 0. If 0, OAT sorts first by highest number of uncancelled runs and then by unpenalized average runtime. Default is 0.",
                                      (int f) => this.InternalConfigurationBuilder.SetFactorParK(f)
                                  },
                                  {
                                      "rngSeed=",
                                      () =>
                                          "The random number generator seed, which generates #numberOfSeeds seeds for every instance of the Lingeling algorithm. Default is 42.",
                                      (int s) => this.InternalConfigurationBuilder.SetRngSeed(s)
                                  },
                                  {
                                      "numberOfSeeds=",
                                      () =>
                                          "Specifies the number of seeds, which are used for every instance of the Lingeling algorithm. Needs to be greater than 0. Default is 1.",
                                      (int n) => this.InternalConfigurationBuilder.SetNumberOfSeeds(n)
                                  },
                                  {
                                      "memoryLimitMegabyte=",
                                      () =>
                                          "Specifies the memory limit (in megabyte), which is used for the algorithm. Needs to be greater than 0. Default is 4000.",
                                      (int m) => this.InternalConfigurationBuilder.SetMemoryLimitMegabyte(m)
                                  },
                              };
            return options;
        }

        #endregion
    }
}