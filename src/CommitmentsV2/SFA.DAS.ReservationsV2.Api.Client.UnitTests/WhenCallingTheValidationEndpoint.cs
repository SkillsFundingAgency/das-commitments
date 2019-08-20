using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;
using Microsoft.Extensions.Logging;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingTheValidationEndpoint
    {
        private ReservationsClientTestFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new ReservationsClientTestFixture();   
        }

        [Test]
        public async Task ThenTheValidateReservationRequestUriIsCorrectlyFormed()
        {
            await _fixture.ValidateReservationRequest();
            _fixture.AssertValidateReservationUriCorrectlyFormed();
        }

        [Test]
        public async Task ThenTheValidateReservationRequestPayloadIsCorrectlyFormed()
        {
            await _fixture.ValidateReservationRequest();
            _fixture.AssertValidateReservationPayloadCorrectlyFormed();
        }

        [TestCase(1234, 120)]
        public async Task ThenTheBulkCreateReservationsRequestUriIsCorrectlyFormed(long accountLegalEntityId, int count)
        {
            await _fixture.BulkCreateReservationsRequest(accountLegalEntityId, (uint)count);
            _fixture.AssertBulkCreateReservationsUriCorrectlyFormed(accountLegalEntityId, (uint)count);
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

                _restHttpClient.Setup(x => x.PostAsJson<object, BulkCreateReservationsResult>(It.IsAny<string>(),
                        It.IsAny<object>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new BulkCreateReservationsResult(new List<Guid>()));

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

            public async Task<ReservationsClientTestFixture> ValidateReservationRequest()
            {
                await _reservationsApiClient.ValidateReservation(_request, new CancellationToken());
                return this;
            }

            public async Task<ReservationsClientTestFixture> BulkCreateReservationsRequest(long accountLegalEntityId, uint count)
            {
                await _reservationsApiClient.BulkCreateReservations(accountLegalEntityId, count, new CancellationToken());
                return this;
            }

            public void AssertValidateReservationUriCorrectlyFormed()
            {
                var expectedUrl = $"{_config.ApiBaseUrl}/api/reservations/validate/{_request.ReservationId}";

                  _restHttpClient.Verify(x => x.Get<ReservationValidationResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()));
            }

            public void AssertBulkCreateReservationsUriCorrectlyFormed(long accountLegalEntityId, uint count)
            {
                var expectedUrl = $"{_config.ApiBaseUrl}/api/reservations/accounts/{accountLegalEntityId}/bulk-create/{count}";

                _restHttpClient.Verify(x => x.PostAsJson<BulkCreateReservationsResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                    It.IsAny<CancellationToken>()));
            }

            public void AssertValidateReservationPayloadCorrectlyFormed()
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