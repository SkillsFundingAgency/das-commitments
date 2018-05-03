using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class PriceHistoryGenerator : Generator
    {
        public async Task<IEnumerable<DbSetupPriceHistory>> Generate(int apprenticeshipsGenerated, long firstNewApprenticeshipId)
        {
            //todo: support injection
            //todo: better handle dates, like we do with DataLockStatus setup
            var apprenticeshipIdsForPriceHistories = RandomIdGroups(firstNewApprenticeshipId, apprenticeshipsGenerated,
                TestDataVolume.PriceHistoryPerApprenticeshipProbability, Enumerable.Repeat);

            await TestLog.Progress($"Generating {apprenticeshipIdsForPriceHistories.Length} PriceHistory's");

            var priceHistories = new Fixture().CreateMany<DbSetupPriceHistory>(apprenticeshipIdsForPriceHistories.Length);

            return priceHistories.Zip(apprenticeshipIdsForPriceHistories, (priceHistory, apprenticeshipId) =>
            {
                // bit nasty -> shouldn't alter source! but soon to go out of scope
                priceHistory.ApprenticeshipId = apprenticeshipId;
                return priceHistory;
            });
        }

        //todo: hand-roll rather than using autofixture? 
        //private DbSetupPriceHistory GenerateDbSetupPriceHistory()
        //{
        //    return new DbSetupPriceHistory
        //    {
                
        //    };
        //}
    }
}