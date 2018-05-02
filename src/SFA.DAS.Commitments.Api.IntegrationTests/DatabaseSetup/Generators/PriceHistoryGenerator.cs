using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Generators
{
    public class PriceHistoryGenerator : Generator
    {
        //public async Task<IEnumerable<DbSetupPriceHistory>> Generate(int numberOfNewApprenticeships,
        //    long firstNewApprenticeshipId, long firstNewPriceHistoryId)
        //{
        //    await TestLog.Progress($"Generating {totalPriceHistoriesToGenerate} PriceHistories");

        //}

        private DbSetupPriceHistory GenerateDbSetupPriceHistory()
        {
            return new DbSetupPriceHistory
            {
                
            };
        }
    }
}