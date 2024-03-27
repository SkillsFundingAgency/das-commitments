using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.ReservationsV2.Api.Types.UnitTests.ReservationsApiClient;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class WhenCallingThePingEndpoint
    {
        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new WhenCallingThePingEndpointFixtures();
            await fixture.Ping();
            fixture.AssertUriCorrectlyFormed();
        }
    }

    public class WhenCallingThePingEndpointFixtures : ReservationsClientTestFixtures
    {
        public Task Ping()
        {
            return ReservationsApiClient.Ping(new CancellationToken());
        }

        public void AssertUriCorrectlyFormed()
        {
            var expectedUrl = $"{Config.ApiBaseUrl}/ping";

            HttpHelper.Verify(x => x.GetAsync<string>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<object>(), It.IsAny<CancellationToken>()));
        }
    }
}
