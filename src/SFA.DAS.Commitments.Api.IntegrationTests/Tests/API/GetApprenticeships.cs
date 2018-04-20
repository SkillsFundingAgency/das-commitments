using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests.API
{
    [TestFixture]
    public sealed class GetApprenticeships
    {
        //todo: if we make optimisation to GetApprenticeships we need to cover the refactoring with integrationtest(s)
        //      which means we have to set up a specific data set
        //todo: check everything returned (s.*) is actually mapped (and therefore used)
        //todo: GetActiveApprenticeships orders by first name & last name. Is that required? If so, might be better to do that in c sharp
        [Test]
        public async Task GetEmployerApprenticeships()
        {
            long employerId = 187;
            var url = $"api/employer/{employerId}/apprenticeships";

            var stopwatch = Stopwatch.StartNew();
            // block on result, rather than awaiting as it gives a more realistic timing
            var result = IntegrationTestServer.Client.GetAsync(url).Result;
            //stopwatch.Elapsed;

            Assert.IsTrue(result.IsSuccessStatusCode);

            var resultsAsString = await result.Content.ReadAsStringAsync();
            var apprenticeships = JsonConvert.DeserializeObject<IEnumerable<Apprenticeship>>(resultsAsString);
        }
    }
}
