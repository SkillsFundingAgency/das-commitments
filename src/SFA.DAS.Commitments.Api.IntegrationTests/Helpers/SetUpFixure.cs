using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    [SetUpFixture]
    public class SetUpFixure
    {
        public static TestIds TestIds { get; set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            try
            {
                await TestContext.Progress.WriteLineAsync("Running OneTimeSetUp");
                TestIds = await new TestData().Initialise();
            }
            catch (Exception exception)
            {
                Assert.Fail($"OneTimeSetup threw exception: {exception}");
            }
        }

        [OneTimeTearDown]
        public void TearDownOneTimeTearDown()
        {
            IntegrationTestServer.Shutdown();
        }
    }
}
