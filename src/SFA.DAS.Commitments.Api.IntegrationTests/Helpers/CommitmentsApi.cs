using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static async Task<TimeSpan> CallGetApprenticeship(long apprenticeshipId, long employerAccountId)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await IntegrationTestServer.Client.GetAsync(
                    $"/api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}");
            var callTime = stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            //bool verifyApprenticeship?
            //var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);

            return callTime;
        }

        public static async Task<TimeSpan> CallGetApprenticeships(long employerAccountId)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await IntegrationTestServer.Client.GetAsync($"/api/employer/{employerAccountId}/apprenticeships");
            var callTime = stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            //bool verify?
            //var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);
            ////sproc GetActiveApprenticeships filters out deleted and pre-approved PaymentStatus'es, so this isn't valid
            ////Assert.AreEqual(TestDataVolume.MaxNumberOfApprenticeshipsInCohort, apprenticeships.Count());
            //Assert.LessOrEqual(apprenticeships.Count(), TestDataVolume.MaxNumberOfApprenticeshipsInCohort); // we can do better than this if required - i.e. store and/or generate status counts

            return callTime;
        }
    }
}
