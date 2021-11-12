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

namespace Optano.Algorithm.Tuner.Gurobi.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Optano.Algorithm.Tuner.GenomeEvaluation.Evaluation;
    using Optano.Algorithm.Tuner.Genomes;
    using Optano.Algorithm.Tuner.Genomes.Values;
    using Optano.Algorithm.Tuner.TargetAlgorithm;
    using Optano.Algorithm.Tuner.TargetAlgorithm.Instances;

    using Shouldly;

    using Xunit;

    /// <summary>
    /// Contains tests for the <see cref="GurobiRunEvaluator"/> class.
    /// </summary>
    [Collection("NonParallel")]
    public class GurobiRunEvaluatorTests : IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GurobiRunEvaluatorTests"/> class.
        /// Behaves like the [TestInitialize] of MSTest framework.
        /// </summary>
        public GurobiRunEvaluatorTests()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the cpu timeout, used in tests.
        /// </summary>
        private static TimeSpan CpuTimeout => TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets the finished gurobi result, used in tests.
        /// </summary>
        private static GurobiResult FinishedGurobiResult => new GurobiResult(
            0,
            0,
            TimeSpan.FromSeconds(15),
            TargetAlgorithmStatus.Finished,
            true,
            true);

        #endregion

        #region Public Methods and Operators

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <summary>
        /// Tests racing due to primary, secondary or quaternary tune criterion for all <see cref="GurobiTertiaryTuneCriterion"/>.
        /// </summary>
        /// <param name="runtimeInSeconds">The runtime in seconds of the first run of genome 2.</param>
        /// <param name="targetAlgorithmStatus">The target algorithm status of the first run of genome 2.</param>
        /// <param name="hasValidSolution">Whether the first run of genome 2 has a valid solution.</param>
        /// <param name="secondGenomeShouldBeCancelledByRacing">Whether we expect genome 2 to get cancelled by racing.</param>
        [Theory]
        [InlineData(15, TargetAlgorithmStatus.Finished, true, false)] // No racing
        [InlineData(15, TargetAlgorithmStatus.Finished, false, true)] // Racing due to primary tune criterion: number of results with valid solution
        [InlineData(15, TargetAlgorithmStatus.CancelledByTimeout, true, true)] // Racing due to secondary tune criterion: number of cancelled results
        [InlineData(31, TargetAlgorithmStatus.CancelledByTimeout, true, true)] // Racing due to quaternary tune criterion: runtime
        public void RacingDueToPrimarySecondaryOrQuaternaryTuneCriterion(
            int runtimeInSeconds,
            TargetAlgorithmStatus targetAlgorithmStatus,
            bool hasValidSolution,
            bool secondGenomeShouldBeCancelledByRacing)
        {
            var firstGenomeStats = GurobiRunEvaluatorTests.CreateGenomeStats(
                1,
                GurobiRunEvaluatorTests.FinishedGurobiResult,
                GurobiRunEvaluatorTests.FinishedGurobiResult);

            var secondGenomeStats = GurobiRunEvaluatorTests.CreateGenomeStats(
                2,
                new GurobiResult(
                    0,
                    0,
                    TimeSpan.FromSeconds(runtimeInSeconds),
                    targetAlgorithmStatus,
                    hasValidSolution,
                    true));

            foreach (GurobiTertiaryTuneCriterion tertiaryTuneCriterion in Enum.GetValues(typeof(GurobiTertiaryTuneCriterion)))
            {
                GurobiRunEvaluatorTests.AssertGenomesThatCanBeCancelledByRacing(
                    tertiaryTuneCriterion,
                    firstGenomeStats,
                    secondGenomeStats,
                    secondGenomeShouldBeCancelledByRacing);
            }
        }

        /// <summary>
        /// Tests racing due to tertiary tune criterion for all <see cref="GurobiTertiaryTuneCriterion"/>.
        /// </summary>
        /// <param name="tertiaryTuneCriterion">The tertiary tune criterion.</param>
        /// <param name="secondGenomeShouldBeCancelledByRacing">Whether we expect the second genome to get cancelled by racing.</param>
        [Theory]
        [InlineData(GurobiTertiaryTuneCriterion.MipGap, true)]
        [InlineData(GurobiTertiaryTuneCriterion.BestObjective, true)]
        [InlineData(GurobiTertiaryTuneCriterion.BestObjectiveBound, true)]
        [InlineData(GurobiTertiaryTuneCriterion.None, false)]
        public void RacingDueToTertiaryTuneCriterion(
            GurobiTertiaryTuneCriterion tertiaryTuneCriterion,
            bool secondGenomeShouldBeCancelledByRacing)
        {
            var firstGenomeStats = GurobiRunEvaluatorTests.CreateGenomeStats(
                1,
                GurobiRunEvaluatorTests.FinishedGurobiResult,
                new GurobiResult(
                    10,
                    -100,
                    GurobiRunEvaluatorTests.CpuTimeout,
                    TargetAlgorithmStatus.CancelledByTimeout,
                    true,
                    true));

            var secondGenomeStats = GurobiRunEvaluatorTests.CreateGenomeStats(
                2,
                new GurobiResult(
                    20,
                    -400,
                    GurobiRunEvaluatorTests.CpuTimeout,
                    TargetAlgorithmStatus.CancelledByTimeout,
                    true,
                    true));

            GurobiRunEvaluatorTests.AssertGenomesThatCanBeCancelledByRacing(
                tertiaryTuneCriterion,
                firstGenomeStats,
                secondGenomeStats,
                secondGenomeShouldBeCancelledByRacing);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates <see cref="ImmutableGenomeStats{I,R}"/> from the given information.
        /// </summary>
        /// <param name="genomeId">The genome ID.</param>
        /// <param name="firstResult">The first <see cref="GurobiResult"/>.</param>
        /// <param name="secondResult">The second <see cref="GurobiResult"/>.</param>
        /// <returns>The <see cref="ImmutableGenomeStats{I,R}"/>.</returns>
        private static ImmutableGenomeStats<InstanceSeedFile, GurobiResult> CreateGenomeStats(
            int genomeId,
            GurobiResult firstResult = null,
            GurobiResult secondResult = null)
        {
            var genome = new Genome();
            genome.SetGene("ID", new Allele<int>(genomeId));

            var firstInstance = new InstanceSeedFile("Instance_1", 1);
            var secondInstance = new InstanceSeedFile("Instance_2", 2);

            var genomeStats = new GenomeStats<InstanceSeedFile, GurobiResult>(
                new ImmutableGenome(genome),
                Enumerable.Empty<InstanceSeedFile>(),
                new List<InstanceSeedFile>
                    {
                        firstInstance,
                        secondInstance,
                    });

            if (firstResult != null)
            {
                genomeStats.FinishInstance(firstInstance, firstResult);
            }

            if (secondResult != null)
            {
                genomeStats.FinishInstance(secondInstance, secondResult);
            }

            return new ImmutableGenomeStats<InstanceSeedFile, GurobiResult>(genomeStats);
        }

        /// <summary>
        /// Asserts genomes that can be cancelled by racing.
        /// </summary>
        /// <param name="tertiaryTuneCriterion">The tertiary tune criterion..</param>
        /// <param name="firstGenomeStats">The first genome stats.</param>
        /// <param name="secondGenomeStats">The second genome stats.</param>
        /// <param name="secondGenomeShouldBeCancelledByRacing">Whether we expect the second genome to get cancelled by racing.</param>
        /// <remarks>
        /// We always expect the first genome not to get cancelled by racing. 
        /// </remarks>
        private static void AssertGenomesThatCanBeCancelledByRacing(
            GurobiTertiaryTuneCriterion tertiaryTuneCriterion,
            ImmutableGenomeStats<InstanceSeedFile, GurobiResult> firstGenomeStats,
            ImmutableGenomeStats<InstanceSeedFile, GurobiResult> secondGenomeStats,
            bool secondGenomeShouldBeCancelledByRacing)
        {
            var genomeStats = new List<ImmutableGenomeStats<InstanceSeedFile, GurobiResult>>
                                  {
                                      firstGenomeStats,
                                      secondGenomeStats,
                                  };

            var genomesThatCanBeCancelledByRacing =
                new GurobiRunEvaluator(GurobiRunEvaluatorTests.CpuTimeout, tertiaryTuneCriterion)
                    .GetGenomesThatCanBeCancelledByRacing(genomeStats, 1);

            if (secondGenomeShouldBeCancelledByRacing)
            {
                var cancelledGenome = genomesThatCanBeCancelledByRacing.Single();
                ImmutableGenome.GenomeComparer.Equals(cancelledGenome, firstGenomeStats.Genome).ShouldBeFalse();
                ImmutableGenome.GenomeComparer.Equals(cancelledGenome, secondGenomeStats.Genome).ShouldBeTrue();
            }
            else
            {
                genomesThatCanBeCancelledByRacing.ShouldBeEmpty();
            }
        }

        #endregion
    }
}