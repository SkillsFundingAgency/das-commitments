using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public static class CommitmentsApi
    {
        public static async Task CallGetApprenticeship(long apprenticeshipId, long employerAccountId)
        {
            var result = await IntegrationTestServer.Client.GetAsync(
                    $"/api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}");

            Assert.IsTrue(result.IsSuccessStatusCode);

            //bool verifyApprenticeship?
            //var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);
        }

        public static async Task CallGetApprenticeships(long employerAccountId)
        {
            var result = await IntegrationTestServer.Client.GetAsync($"/api/employer/{employerAccountId}/apprenticeships");

            Assert.IsTrue(result.IsSuccessStatusCode);

            //bool verify?
            //var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);
            ////sproc GetActiveApprenticeships filters out deleted and pre-approved PaymentStatus'es, so this isn't valid
            ////Assert.AreEqual(TestDataVolume.MaxNumberOfApprenticeshipsInCohort, apprenticeships.Count());
            //Assert.LessOrEqual(apprenticeships.Count(), TestDataVolume.MaxNumberOfApprenticeshipsInCohort); // we can do better than this if required - i.e. store and/or generate statuc counts
        }
    }
}
