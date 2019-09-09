using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingTheValidateReservationEndpoint
    {
        private ReservationsClientTestFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new ReservationsClientTestFixture();   
        }

        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            await _fixture.ValidateRequest();
            _fixture.AssertUriCorrectlyFormed();
        }

        [Test]
        public async Task ThenTheRequestPayloadIsCorrectlyFormed()
        {
            await _fixture.ValidateRequest();
            _fixture.AssertPayloadCorrectlyFormed();
        }

        private class ReservationsClientTestFixture
        {
            private readonly ReservationsApiClient _reservationsApiClient;
            private readonly Mock<IRestHttpClient> _restHttpClient;
            private readonly ReservationsClientApiConfiguration _config;

            private readonly ValidationReservationMessage _request;

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

                var autoFixture = new Fixture();
                _request = new ValidationReservationMessage
                {
                    CourseCode = autoFixture.Create<string>(),
                    ReservationId = autoFixture.Create<Guid>(),
                    StartDate = autoFixture.Create<DateTime>()
                };
            }

            public async Task<ReservationsClientTestFixture> ValidateRequest()
            {
                await _reservationsApiClient.ValidateReservation(_request, new CancellationToken());
                return this;
            }

            public void AssertUriCorrectlyFormed()
            {
                var expectedUrl = $"{_config.ApiBaseUrl}/api/reservations/validate/{_request.ReservationId}";

                  _restHttpClient.Verify(x => x.Get<ReservationValidationResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()));
            }

            public void AssertPayloadCorrectlyFormed()
            {
                var expectedPayload = new
                {
                    StartDate = _request.StartDate.ToString("yyyy-MM-dd"),
                    _request.CourseCode
                };

                _restHttpClient.Verify(x => x.Get<ReservationValidationResult>(It.IsAny<string>(),
                    It.Is<object>(o => CompareHelper.AreEqualIgnoringTypes(expectedPayload, o)),
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