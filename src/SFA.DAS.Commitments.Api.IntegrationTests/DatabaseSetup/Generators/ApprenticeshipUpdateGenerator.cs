using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class ApprenticeshipUpdateGenerator : Generator
    {
        public async Task<IEnumerable<DbSetupApprenticeshipUpdate>> Generate(int apprenticeshipsGenerated, long firstNewApprenticeshipId)
        {
            int apprenticeshipUpdatesToGenerate = (int)(apprenticeshipsGenerated * TestDataVolume.ApprenticeshipUpdatesToApprenticeshipsRatio);
            await TestLog.Progress($"Generating {apprenticeshipUpdatesToGenerate} ApprenticeshipUpdatess");

            //todo: switch to how we decide on datalockstatuses instead?
            var apprenticeshipIdsForUpdates = RandomIdGroups(firstNewApprenticeshipId, apprenticeshipUpdatesToGenerate,
                TestDataVolume.MaxApprenticeshipUpdatesPerApprenticeship, apprenticeshipUpdatesToGenerate);

            var apprenticeshipUpdates = new Fixture().CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate);

            //todo: we need to make sure that where we have ApprenticeshipUpdates with Status=0, then the Originator needs to go into Apprenticeship.PendingOriginator
            //      and when there's no apprenticeshipupdate with status 0, then Apprenticeship.PendingOriginator is null
            //      do we write back over Apprenticeships we've already written, or do we hold apprenticeships in memory until after apprenticeshipupdates are generated
            //      then write both out <- probably the latter, otherwise will take too long (as long as we don't run out of memory!)

            return apprenticeshipUpdates.Zip(apprenticeshipIdsForUpdates, (update, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope
                update.ApprenticeshipId = apprenticeshipId;
                return update;
            });
        }
    }
}
