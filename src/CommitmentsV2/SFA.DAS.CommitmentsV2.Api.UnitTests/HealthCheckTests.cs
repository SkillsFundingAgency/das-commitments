using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.ErrorHandler;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class HealthCheckTests : FluentTest<HealthCheckFixture>
    {
        [Test]
        public Task CallHealthCheckEndpoint_ThenShouldReturnOkResponse()
        {
            return TestAsync(f => f.Client.GetAsync("/api/health-check"), (f,r) => r.StatusCode.Should().Be(HttpStatusCode.OK));
        }

        [Test]
        public Task CallUnknownPage_ThenShouldReturnNotFoundResponse()
        {
            return TestAsync(f => f.Client.GetAsync("/no-such-page"), (f, r) => r.StatusCode.Should().Be(HttpStatusCode.NotFound));
        }
    }

    public class HealthCheckFixture
    {
        public readonly WebApplicationFactory<Startup> Factory;
        public readonly HttpClient Client;

        public HealthCheckFixture()
        {
            Factory = new WebApplicationFactory<Startup>();
            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
    }
}
