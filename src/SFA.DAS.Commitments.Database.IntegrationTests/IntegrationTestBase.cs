using NUnit.Framework;

namespace SFA.DAS.Commitments.Database.IntegrationTests
{
    public class IntegrationTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            TestDatabase.SetupDatabase();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            TestDatabase.DropDatabase();
        }
    }
}
