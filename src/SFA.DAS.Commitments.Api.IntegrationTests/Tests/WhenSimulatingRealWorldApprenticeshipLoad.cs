using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    [TestFixture]
    public class WhenSimulatingRealWorldApprenticeshipLoad
    {
        [Test]
        public async Task ThenSumfinkOrNuffink()
        {
            //todo: the test will have to create these of course mf
            string employerAccountId = "8315";
            string apprenticeshipId = "1";

            var results = await IntegrationTestServer.Client.GetAsync(
                    $"/api/employer/{employerAccountId}/apprenticeships/{apprenticeshipId}");

            var resultsAsString = await results.Content.ReadAsStringAsync();
            var apprenticeship = JsonConvert.DeserializeObject<Apprenticeship>(resultsAsString);
        }
    }
}
