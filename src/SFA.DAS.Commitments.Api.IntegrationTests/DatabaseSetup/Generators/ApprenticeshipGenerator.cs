using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class ApprenticeshipGenerator : Generator
    {
        public async Task<(IEnumerable<DbSetupApprenticeship>, long)> Generate(TestIds testIds, int apprenticeshipsToGenerate, TestDataInjector testDataInjector, int maxCohortSize = TestDataVolume.MaxNumberOfApprenticeshipsInCohort)
        {
            await TestLog.Progress($"Injecting {testDataInjector.Apprenticeships.Count} Apprenticeships");
            apprenticeshipsToGenerate -= testDataInjector.Apprenticeships.Count;

            await TestLog.Progress($"Generating {apprenticeshipsToGenerate} Apprenticeships");
            var apprenticeships = new Fixture().CreateMany<DbSetupApprenticeship>(apprenticeshipsToGenerate);

            // for the first set of apprenticeships, put them in a cohort as big as maxCohortSize (given enough apprenticeships)
            // so that we have a max size cohort for testing purposes.
            // then for the other apprenticeships, give them randomly sized cohorts up to the max
            //todo: get's id that isn't generated if rows are already generated!
            testIds[TestIds.MaxCohortSize] = testDataInjector.NextApprenticeshipId;

            //todo: if we keep tabs on the next id in TestDataInjector, we won't need to return 2 vars
            var firstCohortId = testDataInjector.NextCommitmentId;

            int apprenticeshipsLeftInCohort = maxCohortSize;

            foreach (var apprenticeship in apprenticeships)
            {
                if (--apprenticeshipsLeftInCohort < 0)
                {
                    apprenticeshipsLeftInCohort = Random.Next(1, maxCohortSize + 1);
                    ++firstCohortId;
                }

                apprenticeship.CommitmentId = firstCohortId;
            }

            apprenticeships = testDataInjector.Apprenticeships.Concat(apprenticeships);

            return (apprenticeships, firstCohortId);
        }
    }
}
