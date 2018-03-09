using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.DependencyResolution;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    //todo: infrastructure for integration tests (test helpers/inmemory self hosting) - remove need to authorization for requests from localhost? or in-memory?
    //todo: infrastructure generating mass data(autofixture?/sqlbulkcopy?/schema changes)
    //todo: write scenarios(parallel execution)
    //todo: use INTTEST as env? so could have a seperate database to contain all the test data
    // /\ think we need this so we can blatt the data when necessary

    [TestFixture]
    public class WhenSimulatingRealWorldApprenticeshipLoad
    {
        [Test]
        public async Task SimulateSlowdownScenario()
        {
            //best way to handle concurrency? async only? tpl parallel? threads? other?
            var tasks = new List<Task<TimeSpan>>();

            const int preGetApprenticeshipsGetApprenticeshipCalls = 50;
            const int postGetApprenticeshipsGetApprenticeshipCalls = 50;

            // better to fetch what we need from the generated data from the db, rather than
            // 1) read all the db data into memory (a la provider events api in test), as would use an unnecessarily large amount of memory)
            // 2) store it in the database in a similar way to the test ids, as that would require more code, increase db complexity etc. when you'd have to fetch the data from the db anyway
            var getApprentishipIdsTasks = Enumerable.Repeat(0, preGetApprenticeshipsGetApprenticeshipCalls + postGetApprenticeshipsGetApprenticeshipCalls)
                .Select(async x =>
                {
                    var ApprenticeshipId = GetRandomApprenticeshipId();
                    return new
                    {
                        ApprenticeshipId,
                        EmpoyerId = await SetUpFixure.CommitmentsDatabase.GetEmployerId(ApprenticeshipId)
                    };
                });

            var getApprenticeshipIds = await Task.WhenAll(getApprentishipIdsTasks);

            // pay the cost of test server setup etc. now, so the first result in our timings isn't out
            await CommitmentsApi.CallGetApprenticeship(getApprenticeshipIds.First().ApprenticeshipId, getApprenticeshipIds.First().EmpoyerId);

            //better to just for?
            //for (var preCall = 0; preCall < preGetApprenticeshipsGetApprenticeshipCalls; ++preCall)
            //{
            //}

            //for (var preCall = 0; preCall < preGetApprenticeshipsGetApprenticeshipCalls; ++preCall)
            //{
            //    var apprenticeshipId = GetRandomApprenticeshipId();
            //    tasks.Add(CommitmentsApi.CallGetApprenticeship(apprenticeshipId, apprenticeshipId));
            //}

            tasks.AddRange(getApprenticeshipIds.Take(preGetApprenticeshipsGetApprenticeshipCalls)
                .Select(ids => CommitmentsApi.CallGetApprenticeship(ids.ApprenticeshipId, ids.EmpoyerId)));

            //currently have 1:1 cohort:employer, might have to do better than that, i.e. employer with multiple cohorts, employer with none? perhaps not for our purposes
            tasks.Add(CommitmentsApi.CallGetApprenticeships(SetUpFixure.TestIds[TestIds.MaxCohortSize]));

            //for (var postCall = 0; postCall < postGetApprenticeshipsGetApprenticeshipCalls; ++postCall)
            //{
            //    var apprenticeshipId = GetRandomApprenticeshipId();
            //    tasks.Add(CommitmentsApi.CallGetApprenticeship(apprenticeshipId, apprenticeshipId));
            //}

            tasks.AddRange(getApprenticeshipIds.Skip(preGetApprenticeshipsGetApprenticeshipCalls)
                .Select(ids => CommitmentsApi.CallGetApprenticeship(ids.ApprenticeshipId, ids.EmpoyerId)));

            var callTimes = await Task.WhenAll(tasks);
            var slowestGetApprenticeshipCall = callTimes.Take(preGetApprenticeshipsGetApprenticeshipCalls)
                .Concat(callTimes.Skip(preGetApprenticeshipsGetApprenticeshipCalls+1)).Max(ts => ts);
            var getApprenticechipsCall = callTimes.Skip(preGetApprenticeshipsGetApprenticeshipCalls).First();

            Assert.LessOrEqual(slowestGetApprenticeshipCall, new TimeSpan(0,0,1));
            Assert.LessOrEqual(getApprenticechipsCall, new TimeSpan(0,0,1));
        }

        private static readonly Random Random = new Random();
        public long GetRandomApprenticeshipId()
        {
            long apprenticeshipId;
            while (SetUpFixure.TestIds.Ids.Contains(apprenticeshipId = Random.Next(1, TestDataVolume.MinNumberOfApprenticeships+1)))
                { }
            return apprenticeshipId;
        }

        [Test]
        public async Task ThenSumfinkOrNuffink()
        {
            long apprenticeshipId = SetUpFixure.TestIds[TestIds.MaxCohortSize];
            long employerAccountId = apprenticeshipId; // the convention we're currently using to simplify things

            // when we supply a valid token, in ApiKeyHandler, JwtSecurityTokenHandler.ValidateToken complains that the header isn't base64 encoded,
            // but it is. see https://stackoverflow.com/questions/43003502/jwt-unable-to-decode-the-header-as-base64url-encoded-string
            // have checked that newtonsoft.json is loaded and we have the redirect, and the token is mod 4, but still doesn't work!

            //var httpClient = IntegrationTestServer.Client;
            //Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYyNTk2IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo2MjU3MSIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.bHMfaMfM5ruheC_p97M4jmet_6_MRL_7CoD2uLhKcrk


            //this 1...
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYyNTk2IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo2MjU3MSIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.bHMfaMfM5ruheC_p97M4jmet_6_MRL_7CoD2uLhKcrk");
            //"Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjYyNTk2IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo2MjU3MSIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.bHMfaMfM5ruheC_p97M4jmet_6_MRL_7CoD2uLhKcrk"
            //eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJUb2tlbklzc3VlciIsImF1ZCI6IkF1ZGllbmNlcyIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.NsVVWGXGeeDzPzeS0s_7J0fyc2g_YcPhU36j68qITqg
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEgUm9sZTIiLCJpc3MiOiJUb2tlbklzc3VlciIsImF1ZCI6IkF1ZGllbmNlcyIsImV4cCI6MTg5MjE2MDAwMCwibmJmIjoxNTA3NTQxMTU1fQ.NsVVWGXGeeDzPzeS0s_7J0fyc2g_YcPhU36j68qITqg");
            var results = await IntegrationTestServer.Client.GetAsync(
                    $"/api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}");

            var resultsAsString = await results.Content.ReadAsStringAsync();
            var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);
        }

        [Test]
        public async Task NotSelfHosted()
        {
            //todo: the test will have to create these of course mf
            long employerAccountId = 8315;
            long apprenticeshipId = 1;

            var container = IoC.Initialize();
            container.Configure(c => c.AddRegistry<TestRegistry>());

            var employerController = container.GetInstance<EmployerController>();
            var apprenticeship = await employerController.GetApprenticeship(employerAccountId, apprenticeshipId);
        }
    }
}
