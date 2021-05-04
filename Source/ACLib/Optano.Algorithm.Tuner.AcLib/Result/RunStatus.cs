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
    /// <summary>
    /// Possible stati of a finished target algorithm run.
    /// These follow http://www.cs.ubc.ca/labs/beta/Projects/SMAC/v2.08.00/manual.pdf#page=17.
    /// </summary>
    public enum RunStatus
    {
        /// <summary>
        /// Run was successful (and satisfiable).
        /// </summary>
        Sat,

        /// <summary>
        /// Run was successful (and unsatisfiable).
        /// </summary>
        Unsat,

        /// <summary>
        /// The run has timed out.
        /// </summary>
        Timeout,

        /// <summary>
        /// The target algorithm crashed.
        /// </summary>
        Crashed,

        /// <summary>
        /// The target algorithm ended up in an inconsistent state that should never be reached, and further runs
        /// will also end up in this state.
        /// </summary>
        Abort,
    }
}