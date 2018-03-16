using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    // prefer composition?
    public class Generator
    {
        protected readonly Random Random = new Random();

        public long[] RandomIdGroups(long firstId, int countOfIds, int maxIdsPerGroup, int maxIdsRequired)
        {
            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int)firstId, countOfIds)
                .OrderBy(au => Random.Next());

            // 'lock in' the enumeration by converting to an array, otherwise you get different results for each enumeration!
            return newApprenticeshipIdsShuffled
                .SelectMany(id => Enumerable.Repeat((long)id, Random.Next(1, maxIdsPerGroup + 1)))
                .Take(maxIdsRequired)
                .ToArray();
        }

        //public long[] RandomIdGroups(long firstId, int countOfIds, ProbabilityDistribution<int> probabilityDistribution)
        //{
        //    var newApprenticeshipIdsShuffled = Enumerable
        //        .Range((int)firstId, countOfIds)
        //        .OrderBy(au => Random.Next());

        //    // 'lock in' the enumeration by converting to an array, otherwise you get different results for each enumeration!
        //    return newApprenticeshipIdsShuffled
        //        .SelectMany(id => Enumerable.Repeat((long)id, probabilityDistribution.NextRandom()))
        //        .ToArray();
        //}

        //any need for base anymore? yes, if replace above method consumer with this one
        public T[] RandomIdGroups<T>(long firstId, int countOfIds, ProbabilityDistribution<int> probabilityDistribution,
            Func<long,int,IEnumerable<T>> generateGroup)
        {
            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int)firstId, countOfIds)
                .OrderBy(au => Random.Next());

            // 'lock in' the enumeration by converting to an array, otherwise you get different results for each enumeration!
            return newApprenticeshipIdsShuffled
                .SelectMany(id => generateGroup(id, probabilityDistribution.NextRandom()))
                //{
                //    var groupLength = probabilityDistribution.NextRandom();
                //    func(id, probabilityDistribution.NextRandom())
                //    //Enumerable.Repeat((long)id, groupLength).Select((i,index) => new {Id = i, StartDate = GenerateStartData })
                //})
//            return Enumerable.Repeat((long) id, probabilityDistribution.NextRandom());
                .ToArray();
        }

    }
}
