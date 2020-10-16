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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// An abstract base class for executing arbitrary commands as target algorithms.
    /// </summary>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public abstract class CommandExecutorBase<TResult> : ITargetAlgorithm<InstanceFile, TResult>
        where TResult : ResultBase<TResult>, new()
    {
        #region Constants

        /// <summary>
        /// Part of the <see cref="Command"/> that will be replaced with the path to the instance file.
        /// </summary>
        public const string InstanceReplacement = "{instance}";

        /// <summary>
        /// Part of the <see cref="Command"/> that will be replaced with the parameters in the form of
        /// "-firstIdentifier firstValue -secondIdentifier secondValue ... -nthIdentifier nthValue".
        /// </summary>
        public const string ParameterReplacement = "{arguments}";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExecutorBase{TResult}"/> class.
        /// </summary>
        /// <param name="parameters">Parameters for the target algorithm.</param>
        /// <param name="basicCommand">The basic command to the target algorithm as it should be executed by the
        /// command line. The path to the instance file and the parameters will be set by replacing
        /// <see cref="InstanceReplacement"/> and <see cref="ParameterReplacement"/>.</param>
        protected CommandExecutorBase(Dictionary<string, IAllele> parameters, string basicCommand)
        {
            // Render the parameters as accepted by the command line.
            // Set culture to invariant to make sure doubles use periods, not commas.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            var commandParameters = string.Join(
                " ",
                parameters.Select(parameter => $"-{parameter.Key} {parameter.Value}"));

            // Build up the target algorithm command by using the parameters.
            this.Command = basicCommand.Replace(ParameterReplacement, commandParameters);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configured command to the target algorithm as it should be executed by the command line. The path
        /// to the instance file will be set by replacing <see cref="InstanceReplacement"/>.
        /// </summary>
        protected string Command { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a cancellable task that runs the <see cref="Command"/> on the given instance.
        /// </summary>
        /// <param name="instance">Instance to run on.</param>
        /// <param name="cancellationToken">Token that is regurlarly checked for cancellation.
        /// If cancellation is detected, the task will be stopped.</param>
        /// <returns>A task that returns the run's result on completion.</returns>
        public abstract Task<TResult> Run(InstanceFile instance, CancellationToken cancellationToken);

        #endregion

        #region Methods

        /// <summary>
        /// Builds the <see cref="ProcessStartInfo"/> for starting the algorithm on the given instance.
        /// </summary>
        /// <param name="instance">The instance to start the algorithm on.</param>
        /// <returns>The built <see cref="ProcessStartInfo"/>.</returns>
        protected ProcessStartInfo BuildProcessStartInfo(InstanceFile instance)
        {
            // Create the process information using the correct program and parameters.
            var command = this.Command.Replace(InstanceReplacement, $"\"{instance.Path}\"");
            ProcessStartInfo processInfo = new ProcessStartInfo(
                                               fileName: command.Split(' ').First(),
                                               arguments: command.Substring(command.IndexOf(' ') + 1))
                                               {
                                                   // Make sure no additional window will be opened on process start.
                                                   CreateNoWindow = true,
                                                   UseShellExecute = false,
                                               };

            return processInfo;
        }

        #endregion
    }
}