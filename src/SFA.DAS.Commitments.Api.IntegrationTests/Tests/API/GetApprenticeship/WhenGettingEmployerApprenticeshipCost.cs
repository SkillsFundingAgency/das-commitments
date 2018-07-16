using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

//todo: check onetimesetup is still called for sub namespaces
namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API.GetApprenticeship
{
    [TestFixture]
    public sealed class WhenGettingEmployerApprenticeshipCost
    {
        private const string GetEmployerApprenticeshipEmployerId = "WhenGettingEmployerApprenticeshipCost_AndActiceApprenticeshipWithCurrentPriceHistoryWithToDateThenCurrentPriceHistoryCostIsReturned_EmployerId";
        private const string GetEmployerApprenticeshipApprenticeshipId = "WhenGettingEmployerApprenticeshipCost_AndActiceApprenticeshipWithCurrentPriceHistoryWithToDateThenCurrentPriceHistoryCostIsReturned_ApprenticeshipId";

        private const string FirstName = "GetApprenticeship";
        private const string LastName = "CurrentPriceHistoryClosedEnded";
        private const decimal ApprenticeshipCost = 123L;
        private const decimal ExpectedPriceHistoryCost = 890L;

        public static void InjectTestSpecificData(TestDataInjector injector)
        {
            //todo: clone, to avoid mutations
            var commitmentId = injector.AddCommitment(
                TestDbSetupEntities.GetDbSetupCommitment(),
                GetEmployerApprenticeshipEmployerId);

            var apprenticeship = TestDbSetupEntities.GetDbSetupApprenticeship(commitmentId, FirstName, LastName);

            // this is the cost that we *don't* want returned
            apprenticeship.Cost = ApprenticeshipCost;

            // for cost to be taken from current closed ended PriceHistory...

            // PaymentStatus must not be PendingApproval 
            apprenticeship.PaymentStatus = PaymentStatus.Active;
            // start date must be in past
            apprenticeship.StartDate = DateTime.Now.AddMonths(-3);

            var apprenticeshipId = injector.AddApprenticeship(apprenticeship, GetEmployerApprenticeshipApprenticeshipId);

            injector.AddPriceHistory(new DbSetupPriceHistory
            {
                ApprenticeshipId = apprenticeshipId,
                // this is the cost that we want returned
                Cost = ExpectedPriceHistoryCost,
                // FromDate must be in the past
                FromDate = DateTime.Now.AddMonths(-3),
                // ToDate must be non-null and in the future
                // as long as the data is regenerated within a 4 year window, this should be fine
                ToDate = DateTime.Now.AddYears(4)
            });
        }

        [Test]
        public async Task AndActiceApprenticeshipWithCurrentPriceHistoryWithToDateThenCurrentPriceHistoryCostIsReturned()
        {
            long employerId = TestSetup.TestIds[GetEmployerApprenticeshipEmployerId];
            long apprenticeshipId = TestSetup.TestIds[GetEmployerApprenticeshipApprenticeshipId];
            var url = $"api/employer/{employerId}/apprenticeships/{apprenticeshipId}";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            await TestLog.Progress($"Call to GetEmployerApprenticeship took {stopwatch.Elapsed}");

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);

            // first check we have the correct apprenticeship
            Assert.AreEqual($"{FirstName} {LastName}", apprenticeship.ApprenticeshipName);
            // now check we get the correct cost
            Assert.AreEqual(ExpectedPriceHistoryCost, apprenticeship.Cost);
        }
    }
}