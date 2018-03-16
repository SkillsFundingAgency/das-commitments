using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Tests;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class ApprenticeshipUpdateGenerator : Generator
    {
        public async Task<IEnumerable<DbSetupApprenticeshipUpdate>> Generate(int apprenticeshipsGenerated, long firstNewApprenticeshipId)
        {
            int apprenticeshipUpdatesToGenerate = (int)(apprenticeshipsGenerated * TestDataVolume.ApprenticeshipUpdatesToApprenticeshipsRatio);
            await SetUpFixture.LogProgress($"Generating {apprenticeshipUpdatesToGenerate} ApprenticeshipUpdatess");

            //todo: switch to how we decide on datalockstatuses instead?
            var apprenticeshipIdsForUpdates = RandomIdGroups(firstNewApprenticeshipId, apprenticeshipUpdatesToGenerate,
                TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship, apprenticeshipUpdatesToGenerate);

            var apprenticeshipUpdates = new Fixture().CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate);

            return apprenticeshipUpdates.Zip(apprenticeshipIdsForUpdates, (update, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope
                update.ApprenticeshipId = apprenticeshipId;
                return update;
            });
        }
    }
}
