using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API.CreateCommitment
{
    [TestFixture]
    public class WhenCreatingCommitment
    {
        [Test]
        public async Task ThenCommitmentIsCreated()
        {
            //do we want to insert a particular employer, or add a GetGeneratedEmployerId or similar (to get an employer id what hasn't been rejected, also a new generated one each time)??
            long employerId = 8315; //TestSetup.TestIds[GetEmployerApprenticeshipEmployerId];
            var url = $"api/employer/{employerId}/commitments";

            var commitmentRequest = new CommitmentRequest {Commitment = TestEntities.GetCommitmentForCreate()};

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.PostAsJsonAsync(url, commitmentRequest).Result;
            await TestLog.Progress($"Call to CreateCommitment took {stopwatch.Elapsed}");

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            //var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);
        }
    }
}
