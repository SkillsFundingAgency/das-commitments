using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingThePingEndpoint
    {
        private ReservationsClientTestFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new ReservationsClientTestFixture();
        }
        
        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            await _fixture.Ping();
            _fixture.AssertUriCorrectlyFormed();
        }

        private class ReservationsClientTestFixture
        {
            private readonly ReservationsApiClient _reservationsApiClient;
            private readonly Mock<IRestHttpClient> _restHttpClient;
            private readonly ReservationsClientApiConfiguration _config;

            public ReservationsClientTestFixture()
            {
                _restHttpClient = new Mock<IRestHttpClient>();
                _restHttpClient.Setup(x => x.Get<ReservationValidationResult>(It.IsAny<string>(),
                        It.IsAny<object>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ReservationValidationResult());

                _config = new ReservationsClientApiConfiguration
                {
                    ApiBaseUrl = "https://somehost"
                };

                _reservationsApiClient = new ReservationsApiClient(_restHttpClient.Object, new ReservationHelper(_config), Mock.Of<ILogger<ReservationsApiClient>>());
            }

            public async Task<ReservationsClientTestFixture> Ping()
            {
                await _reservationsApiClient.Ping(new CancellationToken());
                return this;
            }

            public void AssertUriCorrectlyFormed()
            {
                var expectedUrl = $"{_config.ApiBaseUrl}/ping";

                _restHttpClient.Verify(x => x.Get(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()));
            }

            private bool IsSameUri(string expected, string actual)
            {
                var expectedUri = new Uri(expected, UriKind.Absolute);
                var actualUri = new Uri(actual, UriKind.Absolute);

                Assert.AreEqual(expectedUri.Host, actualUri.Host, "Host is wrong");
                Assert.AreEqual(expectedUri.AbsolutePath, actualUri.AbsolutePath, "Path is wrong");
                Assert.AreEqual(expectedUri.Scheme, actualUri.Scheme, "Scheme is wrong");

                return true;
            }
        }
    }
}