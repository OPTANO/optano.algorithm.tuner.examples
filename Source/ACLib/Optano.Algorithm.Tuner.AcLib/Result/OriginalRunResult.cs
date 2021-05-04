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

namespace Optano.Algorithm.Tuner.AcLib.Result
{
    using System;

    /// <summary>
    /// Results of a target algorithm run as reported by its output.
    /// </summary>
    public class OriginalRunResult
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginalRunResult"/> class.
        /// </summary>
        /// <param name="status">The status of the target algorithm run.</param>
        /// <param name="runtime">The runtime of the target algorithm.</param>
        /// <param name="quality">A domain specific measure of the quality of the solution.</param>
        public OriginalRunResult(RunStatus status, TimeSpan runtime, double quality)
        {
            this.Status = status;
            this.Runtime = runtime;
            this.Quality = quality;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the status of the target algorithm run.
        /// </summary>
        public RunStatus Status { get; }

        /// <summary>
        /// Gets the runtime of the target algorithm.
        /// </summary>
        public TimeSpan Runtime { get; }

        /// <summary>
        /// Gets a domain specific measure of the quality of the solution.
        /// Some (runtime-oriented) target algorithms might not set this correctly.
        /// </summary>
        public double Quality { get; }

        #endregion
    }
}