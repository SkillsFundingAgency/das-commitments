using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class CommitmentGenerator
    {
        public async Task<IEnumerable<DbSetupCommitment>> Generate(long lastCohortId, long firstNewCohortId)
        {
            int commitmentsToGenerate = (int)(1 + lastCohortId - firstNewCohortId);
            await TestLog.Progress($"Generating {commitmentsToGenerate} Commitments");

            var commitments = new Fixture().CreateMany<DbSetupCommitment>(commitmentsToGenerate);
            foreach (var commitment in commitments)
            {
                // we'll probably have to do better than this at some point, but this might be enough for the initial tests
                // if we do something a bit closer to real-world, we'll have to add probably 2
                // extra columns to IntegrationTestIds, EmployerId & ProviderId (or possibly 1 column as a (c++ style) union)
                // this gets out of synch if rows in the commitment table are deleted!
                commitment.EmployerAccountId = firstNewCohortId++;
            }
            return commitments;
        }
    }
}
