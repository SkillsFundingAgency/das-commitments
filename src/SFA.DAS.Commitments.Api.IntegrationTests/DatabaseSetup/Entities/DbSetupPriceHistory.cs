using System;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities
{
    public class DbSetupPriceHistory : IDbSetupEntity
    {
        public long ApprenticeshipId { get; set; }
        public decimal Cost { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
