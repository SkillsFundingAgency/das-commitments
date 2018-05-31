using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers.Probability;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    // prefer composition?
    public class Generator
    {
        protected readonly Random Random = new Random();

        //todo: this will either (unless very lucky)
        // leave out the last apprenticeships (where take(max) < total selectmany'ed
        // or return less than max

        public long[] RandomIdGroups(long firstId, int countOfIds, int maxIdsPerGroup, int maxIdsRequired)
        {
            Assert.LessOrEqual(firstId, int.MaxValue);

            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int)firstId, countOfIds)
                .OrderBy(au => Random.Next());

            // 'lock in' the enumeration by converting to an array, otherwise you get different results for each enumeration!
            return newApprenticeshipIdsShuffled
                .SelectMany(id => Enumerable.Repeat((long)id, Random.Next(1, maxIdsPerGroup + 1)))
                .Take(maxIdsRequired)
                .ToArray();
        }

        //any need for base anymore? yes, if replace above method consumer with this one
        public T[] RandomIdGroups<T>(long firstId, int countOfIds, ProbabilityDistribution<int> probabilityDistribution,
            Func<long,int,IEnumerable<T>> generateGroup)
        {
            Assert.LessOrEqual(firstId, int.MaxValue);

            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int)firstId, countOfIds)
                .OrderBy(au => Random.Next());

            // 'lock in' the enumeration by converting to an array, otherwise you get different results for each enumeration!
            return newApprenticeshipIdsShuffled
                .SelectMany(id => generateGroup(id, probabilityDistribution.NextRandom()))
                .ToArray();
        }
    }
}
