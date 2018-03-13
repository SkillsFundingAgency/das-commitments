using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public class CallDetails
    {
        public DateTime StartTime { get; set; }
        public TimeSpan CallTime { get; set; }
        public int ThreadId { get; set; }
    }

    public static class CommitmentsApi
    {
        // notes on including authorisation in test when calling in-memory self-hosted service:
        // we currently use AuthorizeRemoteOnly to bypass authorization for local calls, but we did try testing including authorization, but
        // when we supply a valid token, in ApiKeyHandler, JwtSecurityTokenHandler.ValidateToken complains that the header isn't base64 encoded,
        // but it is. see https://stackoverflow.com/questions/43003502/jwt-unable-to-decode-the-header-as-base64url-encoded-string
        // have checked that newtonsoft.json is loaded and we have the redirect, and the token is mod 4, but still doesn't work!

        // this is how to supply the jwt...
        // httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYyNTk2IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo2MjU3MSIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.bHMfaMfM5ruheC_p97M4jmet_6_MRL_7CoD2uLhKcrk");

        public static async Task<CallDetails> CallGetApprenticeship(long apprenticeshipId, long employerAccountId)
        {
            var callDetails = new CallDetails
            {
                StartTime = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
            };
            var stopwatch = Stopwatch.StartNew();
            var result = await IntegrationTestServer.Client.GetAsync(
                    $"/api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}");
            callDetails.CallTime = stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            //bool verifyApprenticeship?
            //var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);

            return callDetails;
        }

        //public static async Task<CallDetails[]> CallGetApprenticeships(ICollection<long> employerAccountIds)
        //{
        //    var tasks = employerAccountIds.Select(CallGetApprenticeships);
        //    return await Task.WhenAll(tasks);
        //}

        public static async Task<CallDetails> CallGetApprenticeships(long employerAccountId)
        {
            var callDetails = new CallDetails
            {
                StartTime = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
            };
            var stopwatch = Stopwatch.StartNew();
            var result = await IntegrationTestServer.Client.GetAsync($"/api/employer/{employerAccountId}/apprenticeships");
            callDetails.CallTime = stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            //bool verify?
            var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);
            ////sproc GetActiveApprenticeships filters out deleted and pre-approved PaymentStatus'es, so this isn't valid
            ////Assert.AreEqual(TestDataVolume.MaxNumberOfApprenticeshipsInCohort, apprenticeships.Count());
            //Assert.LessOrEqual(apprenticeships.Count(), TestDataVolume.MaxNumberOfApprenticeshipsInCohort); // we can do better than this if required - i.e. store and/or generate status counts

            return callDetails;
        }
    }
}
