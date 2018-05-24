using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        public static TestIds TestIds { get; private set; }
        public static CommitmentsDatabase CommitmentsDatabase { get; private set; }
        public static CommitmentsApiConfiguration Configuration { get; private set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            try
            {
                //Debug.Listeners.Add(new DefaultTraceListener());
                await TestLog.Progress("Running OneTimeSetUp");

                Configuration = Infrastructure.Configuration.Configuration.Get();

                var testData = new TestData(Configuration);
                TestIds = await testData.Initialise();
                CommitmentsDatabase = testData.CommitmentsDatabase;

                // pay the cost of test server setup etc. now, so the first result in our timings isn't out
                //todo: get 401 when called from here, but not in test!
                //var randomApprenticeshipId = await CommitmentsDatabase.GetRandomApprenticeshipIds().First();
                //await CommitmentsApi.CallGetApprenticeship(randomApprenticeshipId, randomApprenticeshipId);
            }
            catch (Exception exception)
            {
                Assert.Fail($"OneTimeSetup threw exception: {exception}");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            IntegrationTestServer.Shutdown();
        }
    }
}
