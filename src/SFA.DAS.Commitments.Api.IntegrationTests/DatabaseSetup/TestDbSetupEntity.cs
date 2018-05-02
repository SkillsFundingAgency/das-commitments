using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class TestDbSetupEntity
    {
        // leave null if you don't require the id of this particular IDbSetupEntity for your integration test
        public string TestIdName { get; set; }
        public IDbSetupEntity DbSetupEntity { get; set; }
    }
}
