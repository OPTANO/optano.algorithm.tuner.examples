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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System.Threading;

    using global::Gurobi;

    /// <summary>
    ///     Responsible for defining what Gurobi should do on callbacks.
    /// </summary>
    internal class GurobiCallback : GRBCallback
    {
        #region Fields

        /// <summary>
        ///     The cancellation token issued by OPTANO Algorithm Tuner.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GurobiCallback" /> class.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token issued by OPTANO Algorithm Tuner.</param>
        public GurobiCallback(CancellationToken cancellationToken)
        {
            this._cancellationToken = cancellationToken;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Custom callback implementation that aborts the optimization when the
        ///     <see cref="_cancellationToken" /> requests it.
        /// </summary>
        protected override void Callback()
        {
            if (this._cancellationToken.IsCancellationRequested)
            {
                this.Abort();
            }
        }

        #endregion
    }
}