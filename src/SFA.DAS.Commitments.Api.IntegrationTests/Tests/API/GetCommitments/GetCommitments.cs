using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API.GetCommitments
{
    [TestFixture]
    public sealed class GetCommitments
    {
        //private ICommitmentsApiClientConfiguration _clientConfig;

        [SetUp]
        public void Setup()
        {
            //_clientConfig = A.Fake<ICommitmentsApiClientConfiguration>();
        }

        [Test]
        [Ignore("Not Implemented Yet")]
        public void GetProviderCommitmentsUsingClient()
        {
            //var api = new ProviderCommitmentsApi(IntegrationTestServer.Client, _clientConfig);

            //var commitments = await api.GetProviderCommitments(1);
        }

        [Test]
        public async Task GetProviderCommitments()
        {
            //todo: generate messages (for each commitment)
            //todo: either pick random provider and commitmentstatus (getcommitments filters out CommitmentStatus <> 2 (deleted)
            //      or add known commitment with given data and provider id and store in int test ids.
            //      ^^ allow a test to 'plug in' creation of a specific entity (would be nice, possibly required!)

            long providerId = 187;
            var url = $"/api/provider/{providerId}/commitments";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            //stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var commitments = JsonConvert.DeserializeObject<List<CommitmentListItem>>(resultsAsString);
        }
    }
}
