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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Optano.Algorithm.Tuner.GenomeEvaluation.Evaluation;
    using Optano.Algorithm.Tuner.Genomes;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;
    using Optano.Algorithm.Tuner.TargetAlgorithm.RunEvaluators;

    /// <summary>
    /// An implementation of <see cref="IRunEvaluator{TInstance,TResult}" /> that sorts genomes according to their <see cref="GurobiResult"/>s.
    /// </summary>
    public class GurobiRunEvaluator : IRunEvaluator<InstanceSeedFile, GurobiResult>
    {
        #region Public Methods and Operators

        /// <inheritdoc />
        public IEnumerable<ImmutableGenomeStats<InstanceSeedFile, GurobiResult>> Sort(
            IEnumerable<ImmutableGenomeStats<InstanceSeedFile, GurobiResult>> allGenomeStatsOfMiniTournament)
        {
            /* This implementation uses the following sorting criteria:

            1.) The higher the number of results that have a valid solution, the better.
            2.) The lower the number of cancelled results, the better.
            3.) The lower the averaged mip gap of the cancelled results, the better.
            4.) The lower the averaged runtime, the better.

            NOTE: No need to penalize the average runtime, since the number of cancelled results is a superior sorting criterion.*/

            return allGenomeStatsOfMiniTournament
                .OrderByDescending(gs => gs.FinishedInstances.Values.Count(result => result.HasValidSolution))
                .ThenBy(gs => gs.FinishedInstances.Values.Count(result => result.IsCancelled))
                .ThenBy(
                    gs => gs.FinishedInstances.Values
                        .Where(result => result.IsCancelled)
                        .Select(result => result.Gap)
                        .DefaultIfEmpty(0)
                        .Average())
                .ThenBy(
                    gs => gs.FinishedInstances.Values
                        .Select(result => result.Runtime.TotalSeconds)
                        .DefaultIfEmpty(double.PositiveInfinity)
                        .Average());
        }

        /// <inheritdoc />
        public IEnumerable<ImmutableGenome> GetGenomesThatCanBeCancelledByRacing(
            IReadOnlyList<ImmutableGenomeStats<InstanceSeedFile, GurobiResult>> allGenomeStatsOfMiniTournament,
            int numberOfTournamentWinners)
        {
            var canBeCancelledByRacing = new List<ImmutableGenome>();

            var racingCandidate = this.Sort(allGenomeStatsOfMiniTournament).Skip(numberOfTournamentWinners - 1).First();
            var minimumNumberOfValidResultsOfRacingCandidate = racingCandidate.FinishedInstances.Values.Count(result => result.HasValidSolution);
            var maximumNumberOfCancelledResultsOfRacingCandidate = racingCandidate.FinishedInstances.Values.Count(result => result.IsCancelled)
                                                                   + racingCandidate.OpenInstances.Count + racingCandidate.RunningInstances.Count;

            foreach (var genomeStats in allGenomeStatsOfMiniTournament.Where(g => !g.IsCancelledByRacing && g.HasOpenOrRunningInstances))
            {
                var maximumNumberOfValidResults = genomeStats.FinishedInstances.Values.Count(result => result.HasValidSolution)
                                                  + genomeStats.OpenInstances.Count + genomeStats.RunningInstances.Count;
                var minimumNumberOfCancelledResults = genomeStats.FinishedInstances.Values.Count(result => result.IsCancelled);

                if (maximumNumberOfValidResults < minimumNumberOfValidResultsOfRacingCandidate)
                {
                    // Cancel by racing, because the current genome cannot have more valid results than the racing candidate.
                    canBeCancelledByRacing.Add(genomeStats.Genome);
                }

                if (maximumNumberOfValidResults == minimumNumberOfValidResultsOfRacingCandidate
                    && minimumNumberOfCancelledResults > maximumNumberOfCancelledResultsOfRacingCandidate)
                {
                    // Cancel by racing, because the current genome cannot have less cancelled results than the racing candidate.
                    canBeCancelledByRacing.Add(genomeStats.Genome);
                }

                if (racingCandidate.AllInstancesFinishedWithoutCancelledResult
                    && genomeStats.RuntimeOfFinishedInstances > racingCandidate.RuntimeOfFinishedInstances)
                {
                    // Cancel by racing, because the current genome cannot finish all instances faster than the racing candidate did without getting any cancelled result.
                    canBeCancelledByRacing.Add(genomeStats.Genome);
                }
            }

            return canBeCancelledByRacing;
        }

        /// <inheritdoc />
        public double ComputeEvaluationPriorityOfGenome(ImmutableGenomeStats<InstanceSeedFile, GurobiResult> genomeStats, TimeSpan cpuTimeout)
        {
            if (genomeStats.IsCancelledByRacing)
            {
                return 1000;
            }

            // First decision criterion: The higher the cancelled instance rate, the later the genome will start.
            var cancelledCount = genomeStats.FinishedInstances.Values.Count(r => r.IsCancelled);
            var cancelledInstanceRate = (double)cancelledCount / genomeStats.TotalInstanceCount;

            // Second decision criterion: The higher the running instance rate, the later the genome will start.
            var runningInstanceRate = (double)genomeStats.RunningInstances.Count / genomeStats.TotalInstanceCount;

            // Third decision criterion: The higher the total runtime rate, the later the genome will start.
            var totalRuntimeRate = genomeStats.RuntimeOfFinishedInstances.TotalMilliseconds
                                   / (genomeStats.TotalInstanceCount * cpuTimeout.TotalMilliseconds);

            RunEvaluatorUtils.CheckIfRateIsOutOfBounds(cancelledInstanceRate, nameof(cancelledInstanceRate));
            RunEvaluatorUtils.CheckIfRateIsOutOfBounds(runningInstanceRate, nameof(runningInstanceRate));
            RunEvaluatorUtils.CheckIfRateIsOutOfBounds(totalRuntimeRate, nameof(totalRuntimeRate));

            var priority = (100 * cancelledInstanceRate) + (10 * runningInstanceRate) + (1 * totalRuntimeRate);

            // The lower the priority, the earlier the genome will start.
            return priority;
        }

        #endregion
    }
}