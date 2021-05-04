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

namespace Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Runtime
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.Result;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A <see cref="RunnerBase{TResult}"/> creating <see cref="RuntimeResult"/>s.
    /// </summary>
    public class RuntimeRunner : RunnerBase<RuntimeResult>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeRunner"/> class.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameters">Parameters for the target algorithm.</param>
        public RuntimeRunner(Scenario scenario, Dictionary<string, IAllele> parameters)
            : base(scenario, parameters)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles a cancellation provoked by a cancellation token.
        /// </summary>
        /// <param name="process">The process that was cancelled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override void HandleCancellation(Process process, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Creates a <see cref="RuntimeResult"/>.
        /// </summary>
        /// <param name="originalRunResult">Results of the target algorithm run as reported by its output.</param>
        /// <returns>The created result.</returns>
        protected override RuntimeResult CreateRunResult(OriginalRunResult originalRunResult)
        {
            // EPMs might report SAT instead of TIMEOUT if the runtime is exactly the cutoff.
            // The specified timeout handling actually mirrors the one of SMAC 2.08 (without SMAC's own cutoff).
            if (originalRunResult.Status == RunStatus.Timeout
                || originalRunResult.Status == RunStatus.Crashed
                || originalRunResult.Runtime >= this.Scenario.CutoffTime)
            {
                return ResultBase<RuntimeResult>.CreateCancelledResult(this.Scenario.CutoffTime);
            }

            return new RuntimeResult(originalRunResult.Runtime);
        }

        #endregion
    }
}