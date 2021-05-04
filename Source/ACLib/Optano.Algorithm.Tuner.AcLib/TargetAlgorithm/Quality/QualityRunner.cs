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

namespace Optano.Algorithm.Tuner.AcLib.TargetAlgorithm.Quality
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Optano.Algorithm.Tuner.AcLib.Configuration;
    using Optano.Algorithm.Tuner.AcLib.Result;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Results;

    /// <summary>
    /// A <see cref="RunnerBase{TResult}"/> creating <see cref="ContinuousResult"/>s.
    /// </summary>
    public class QualityRunner : RunnerBase<ContinuousResult>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QualityRunner"/> class.
        /// </summary>
        /// <param name="scenario">The ACLib scenario.</param>
        /// <param name="parameters">Parameters for the target algorithm.</param>
        public QualityRunner(Scenario scenario, Dictionary<string, IAllele> parameters)
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
            this.WriteProcessOutputToFiles(process);
            throw new InvalidOperationException(
                "Cancelled by OPTANO Algorithm Tuner, but should have cancelled itself!");
        }

        /// <summary>
        /// Creates a <see cref="ContinuousResult"/>.
        /// </summary>
        /// <param name="originalRunResult">Results of the target algorithm run as reported by its output.</param>
        /// <returns>The created result.</returns>
        protected override ContinuousResult CreateRunResult(OriginalRunResult originalRunResult)
        {
            if (originalRunResult.Status == RunStatus.Crashed)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(originalRunResult),
                    "Target algorithm quality run has CRASHED.");
            }

            return new ContinuousResult(originalRunResult.Quality, originalRunResult.Runtime);
        }

        #endregion
    }
}