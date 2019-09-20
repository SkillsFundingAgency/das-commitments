using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.IntegrationTests.Pipeline
{
    [TestFixture]
    [Parallelizable]
    public class HealthCheckTests : FluentTest<HealthCheckFixture>
    {
        [Test]
        public Task CallPingEndpoint_ThenShouldReturnOkResponse()
        {
            return TestAsync(f => f.Client.GetAsync("/api/ping"), (f, r) => r.StatusCode.Should().Be(HttpStatusCode.OK));
        }
    }

    public class HealthCheckFixture
    {
        public readonly WebApplicationFactory<Startup> Factory;
        public readonly HttpClient Client;

        public HealthCheckFixture()
        {
            Factory = new CustomWebApplicationFactory<Startup>();
            
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
    }
}