using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API.GetApprenticeships
{
    [TestFixture]
    public sealed class WhenGettingEmployerApprenticeships
    {
        // by using the convention TestClassName_TestName_IdType, we don't have to centrally store TestId names in TestId as const strings
        private const string GetEmployerApprenticeshipsEmployerId = "WhenGettingEmployerApprenticeships_ThenCoreDetailsAreReturned_EmployerId";

        public static void InjectTestSpecificData(TestDataInjector injector)
        {
            //todo: clone, to avoid mutations
            var commitmentId = injector.AddCommitment(
                TestEntities.GetDbSetupCommitment(),
                GetEmployerApprenticeshipsEmployerId);

            var apprenticeship = TestEntities.GetDbSetupApprenticeship(commitmentId, "A", nameof(WhenGettingEmployerApprenticeships));

            // must not be PendingApproval or Deleted to be returned
            apprenticeship.PaymentStatus = PaymentStatus.Active;
            injector.AddApprenticeship(apprenticeship);

            apprenticeship = TestEntities.GetDbSetupApprenticeship(commitmentId, "B", nameof(WhenGettingEmployerApprenticeships));

            // must not be PendingApproval or Deleted to be returned
            apprenticeship.PaymentStatus = PaymentStatus.Active;
            injector.AddApprenticeship(apprenticeship);
        }

        [Test]
        public async Task ThenCoreDetailsAreReturned()
        {
            long employerId = TestSetup.TestIds[GetEmployerApprenticeshipsEmployerId];
            var url = $"api/employer/{employerId}/apprenticeships";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            //stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);

            //todo:
            //CollectionAssert.AreEquivalent();
            Assert.AreEqual("A", apprenticeships.First().FirstName);
            Assert.AreEqual("B", apprenticeships.Skip(1).First().FirstName);
        }
    }
}
