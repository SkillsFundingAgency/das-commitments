using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    // prefer composition?
    public class Generator
    {
        protected readonly Random Random = new Random();

        public IEnumerable<long> RandomIdGroups(long firstId, int countOfIds, int maxIdsPerGroup)
        {
            // limit length? does it matter if lazily enumerated and don't read past required?
            var newApprenticeshipIdsShuffled = Enumerable
                .Range((int)firstId, countOfIds)
                .OrderBy(au => Random.Next(int.MaxValue));

            return newApprenticeshipIdsShuffled.SelectMany(id => Enumerable.Repeat((long)id, Random.Next(1, maxIdsPerGroup + 1)));
        }
    }
}
