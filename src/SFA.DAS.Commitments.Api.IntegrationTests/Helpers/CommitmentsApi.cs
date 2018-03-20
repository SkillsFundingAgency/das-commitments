using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        // notes on including authorisation in test when calling in-memory self-hosted service:
        // we currently use AuthorizeRemoteOnly to bypass authorization for local calls, but we did try testing including authorization, but
        // when we supply a valid token, in ApiKeyHandler, JwtSecurityTokenHandler.ValidateToken complains that the header isn't base64 encoded,
        // but it is. see https://stackoverflow.com/questions/43003502/jwt-unable-to-decode-the-header-as-base64url-encoded-string
        // have checked that newtonsoft.json is loaded and we have the redirect, and the token is mod 4, but still doesn't work!

        // this is how to supply the jwt...
        // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYyNTk2IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo2MjU3MSIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.bHMfaMfM5ruheC_p97M4jmet_6_MRL_7CoD2uLhKcrk");

        public static async Task<CallDetails> CallGetApprenticeship(long apprenticeshipId, long employerAccountId, bool verifyContent = false)
        {
            var callDetails = new CallDetails
            {
                Name = "GetApprenticeship",
                StartTime = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(
                $"/api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}").Result;
            callDetails.CallTime = stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            if (verifyContent)
            {
                var resultsAsString = await result.Content.ReadAsStringAsync();
                var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);
                Assert.AreEqual(apprenticeshipId, apprenticeship.Id);
                Assert.AreEqual(employerAccountId, apprenticeship.EmployerAccountId);
            }

            return callDetails;
        }

        public static async Task<CallDetails> CallGetApprenticeships(long employerAccountId, bool verifyContent = false)
        {
            var callDetails = new CallDetails
            {
                Name = "GetApprenticeships",
                StartTime = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync($"/api/employer/{employerAccountId}/apprenticeships").Result;
            callDetails.CallTime = stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            if (!verifyContent)
            {
                var resultsAsString = await result.Content.ReadAsStringAsync();
                var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);
                // sproc GetActiveApprenticeships filters out deleted and pre-approved PaymentStatus'es, so we check this...
                // (we could do better than this if required - i.e. store and/or generate status counts)
                Assert.LessOrEqual(apprenticeships.Count(), TestDataVolume.MaxNumberOfApprenticeshipsInCohort);
                Assert.IsTrue(apprenticeships.All(a => a.EmployerAccountId == employerAccountId));
            }

            return callDetails;
        }
    }
}
