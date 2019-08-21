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
    internal class ReservationsClientTestFixture
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

            _restHttpClient.Setup(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(
                    It.IsAny<string>(),
                    It.IsAny<BulkCreateReservationsRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BulkCreateReservationsResult(new List<Guid>()));

            _config = new ReservationsClientApiConfiguration
            {
                ApiBaseUrl = "https://somehost"
            };

            _reservationsApiClient = new ReservationsApiClient(_restHttpClient.Object, new ReservationHelper(_config),
                Mock.Of<ILogger<ReservationsApiClient>>());

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

        public async Task<ReservationsClientTestFixture> BulkCreateReservationsRequest(long accountLegalEntityId,
            BulkCreateReservationsRequest request)
        {
            await _reservationsApiClient.BulkCreateReservations(accountLegalEntityId, request, new CancellationToken());
            return this;
        }

        public void AssertValidateReservationUriCorrectlyFormed()
        {
            var expectedUrl = $"{_config.ApiBaseUrl}/api/reservations/validate/{_request.ReservationId}";

            _restHttpClient.Verify(x => x.Get<ReservationValidationResult>(
                It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()));
        }

        public void AssertBulkCreateReservationsUriCorrectlyFormed(long accountLegalEntityId)
        {
            var expectedUrl = $"{_config.ApiBaseUrl}/api/accounts/{accountLegalEntityId}/bulk-create";

            _restHttpClient.Verify(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(
                It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<BulkCreateReservationsRequest>(),
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

        public void AssertBulkCreateReservationsPayloadCorrectlyPassedToMethod(BulkCreateReservationsRequest request)
        {
            _restHttpClient.Verify(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(
                It.IsAny<string>(),
                request,
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
