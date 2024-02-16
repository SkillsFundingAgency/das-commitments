using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.ReservationsV2.Api.Types.UnitTests.ReservationsApiClient;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class WhenValidatingReservation
    {
        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new WhenRetrievingStatusTestFixturesTestFixtures();
            await fixture.ValidateReservation();
            fixture.AssertUriCorrectlyFormed();
        }

        [Test]
        public async Task ThenTheRequestPayloadIsCorrectlyFormed()
        {
            var fixture = new WhenRetrievingStatusTestFixturesTestFixtures();
            await fixture.ValidateReservation();
            fixture.AssertPayloadCorrectlyFormed();
        }
    }

    public class WhenRetrievingStatusTestFixturesTestFixtures : ReservationsClientTestFixtures
    {
        private readonly ReservationValidationMessage _request;

        public WhenRetrievingStatusTestFixturesTestFixtures()
        {
            HttpHelper.Setup(x => x.GetAsync<ReservationValidationResult>(It.IsAny<string>(),
                    It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReservationValidationResult());

            _request = new ReservationValidationMessage
            {
                CourseCode = AutoFixture.Create<string>(),
                ReservationId = AutoFixture.Create<Guid>(),
                StartDate = AutoFixture.Create<DateTime>()
            };
        }

        public Task ValidateReservation()
        {
            return ReservationsApiClient.ValidateReservation(_request, new CancellationToken());
        }

        public void AssertUriCorrectlyFormed()
        {
            var expectedUrl = $"{Config.ApiBaseUrl}/api/reservations/validate/{_request.ReservationId}";

            HttpHelper.Verify(x => x.GetAsync<ReservationValidationResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<object>(), It.IsAny<CancellationToken>()));
        }

        public void AssertPayloadCorrectlyFormed()
        {
            var expectedPayload = new
            {
                StartDate = _request.StartDate.ToString("yyyy-MM-dd"),
                _request.CourseCode
            };

            HttpHelper.Verify(x => x.GetAsync<ReservationValidationResult>(It.IsAny<string>(),
                It.Is<object>(o => CompareHelper.AreEqualIgnoringTypes(expectedPayload, o)), It.IsAny<CancellationToken>()));
        }
    }
}
