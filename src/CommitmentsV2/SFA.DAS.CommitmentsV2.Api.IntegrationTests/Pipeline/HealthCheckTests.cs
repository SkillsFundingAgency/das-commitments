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
        public Task CallHealthCheckEndpoint_ThenShouldReturnOkResponse()
        {
            return TestAsync(f => f.Client.GetAsync("/api/health-check"),
                (f, r) => r.StatusCode.Should().Be(HttpStatusCode.OK));
        }

        [Test]
        public Task CallUnknownEndpoint_ThenShouldReturnNotFoundResponse()
        {
            return TestAsync(f => f.Client.GetAsync("/no-such-page"),
                (f, r) => r.StatusCode.Should().Be(HttpStatusCode.NotFound));
        }


        [Test]
        public Task CallSecureEndpoint_ThenShouldReturnUnauthorisedResponse()
        {
            return TestAsync(f => f.Client.GetAsync("/api/test"),
                (f, r) => r.StatusCode.Should().Be(HttpStatusCode.Unauthorized));
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