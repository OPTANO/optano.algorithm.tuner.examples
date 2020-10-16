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

namespace Optano.Algorithm.Tuner.Gurobi
{
    using System.Collections.Generic;
    using System.Linq;

    using Optano.Algorithm.Tuner.Genomes;
    using Optano.Algorithm.Tuner.TargetAlgorithm.RunEvaluators;

    /// <summary>
    ///     Responsible for sorting <see cref="Genome" />s according to their <see cref="GurobiResult" />s.
    /// </summary>
    public class GurobiRunEvaluator : IRunEvaluator<GurobiResult>
    {
        #region Public Methods and Operators

        /// <summary>
        /// Sorts the genomes by results, best genome first.
        /// In this case, we first solve by number solved instances, then by number not cancelled instances, then by
        /// average MIP gap of cancelled instances and finally by average runtime.
        /// </summary>
        /// <param name="runResults">Results from target algorithm runs, grouped by genome.</param>
        /// <returns>The sorted genomes, best genomes first.</returns>
        public IEnumerable<ImmutableGenome> Sort(Dictionary<ImmutableGenome, IEnumerable<GurobiResult>> runResults)
        {
            /*
             *
             When changing this ordering, be aware: RuntimeTuning might cancel running tasks when the
             the requestes number of winners (of the minitournament) has their tasks successfully completed.

             Use Parameter
             master --tuneForRunTime=false
             if you'd like to switch RuntimeTuning off.

             */
            return runResults
                .OrderByDescending(genome => genome.Value.Average(result => result.HasValidSolution ? 1d : 0))
                .ThenBy(genome => genome.Value.Average(result => result.IsCancelled ? 1d : 0))
                .ThenBy(
                    genome =>
                        genome.Value
                            .Where(result => result.IsCancelled)
                            .Select(result => result.Gap)
                            .DefaultIfEmpty(0)
                            .Average())
                .ThenBy(genome => genome.Value.Average(result => result.Runtime.TotalMilliseconds))
                .Select(genomePair => genomePair.Key).ToList();
        }

        #endregion
    }
}