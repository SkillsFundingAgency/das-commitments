using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace SFA.DAS.CommitmentsV2.Api.IntegrationTests.Pipeline
{
    [TestFixture]
    [Parallelizable]
    public class HealthCheckTests
    {
        [Test]
        public async Task CallPingEndpoint_ThenShouldReturnOkResponse()
        {
            var fixture = new HealthCheckFixture();
            var message = await fixture.Client.GetAsync("/api/ping");
            
            message.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    public class HealthCheckFixture
    {
        public readonly WebApplicationFactory<Startup> Factory;
        public readonly HttpClient Client;

        public HealthCheckFixture()
        {
            Factory = new CustomWebApplicationFactory<Startup>();
            
            //Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            //{
            //    AllowAutoRedirect = false
            //});

            Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });


        }
    }
}