using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class ApprenticeshipGenerator : Generator
    {
        public async Task<(IEnumerable<DbSetupApprenticeship>, long)> Generate(TestIds testIds, int apprenticeshipsToGenerate, long initialId = 1, long firstCohortId = 1, int maxCohortSize = TestDataVolume.MaxNumberOfApprenticeshipsInCohort)
        {
            await SetUpFixture.LogProgress($"Generating {apprenticeshipsToGenerate} Apprenticeships");

            var apprenticeships = new Fixture().CreateMany<DbSetupApprenticeship>(apprenticeshipsToGenerate);

            // for the first set of apprenticeships, put them in a cohort as big as maxCohortSize (given enough apprenticeships)
            // so that we have a max size cohort for testing purposes.
            // then for the other apprenticeships, give them randomly sized cohorts up to the max
            //todo: get's id that isn't generated if rows are already generated!
            testIds[TestIds.MaxCohortSize] = initialId;

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

            return (apprenticeships, firstCohortId);
        }
    }
}
