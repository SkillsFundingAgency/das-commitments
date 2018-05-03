using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API
{
    [TestFixture]
    public sealed class GetApprenticeship
    {
        [Test]
        public async Task GetEmployerApprenticeship()
        {
            //todo: inject
            long employerId = 1; //TestSetup.TestIds[GetEmployerApprenticeshipsEmployerId];
            long apprenticeshipId = 1;
            var url = $"api/employer/{employerId}/apprenticeships/{apprenticeshipId}";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            //stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);

            //todo:
        }
    }
}