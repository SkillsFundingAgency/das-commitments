using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types;
using SFA.DAS.ReservationsV2.Api.Types.UnitTests.ReservationsApiClient;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class WhenCreatingChangeOfPartyReservation
    {
        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new WhenCreatingChangeOfPartyReservationFixture();
            await fixture.CreateChangeOfPartyReservationRequest();
            fixture.AssertUriCorrectlyFormed();
        }

        [Test]
        public async Task ThenTheRequestPayloadIsPassedInCorrectly()
        {
            var fixture = new WhenCreatingChangeOfPartyReservationFixture();
            await fixture.CreateChangeOfPartyReservationRequest();
            fixture.AssertPayloadIsPassedInCorrectly();
        }
    }

    public class WhenCreatingChangeOfPartyReservationFixture : ReservationsClientTestFixtures
    {
        private readonly CreateChangeOfPartyReservationRequest _request;
        private readonly Guid _reservationId = Guid.NewGuid();

        public WhenCreatingChangeOfPartyReservationFixture()
        {
            HttpHelper.Setup(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(It.IsAny<string>(),
                    It.IsAny<BulkCreateReservationsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BulkCreateReservationsResult(new List<Guid>()));

            _request = AutoFixture.Create<CreateChangeOfPartyReservationRequest>();
        }

        public Task CreateChangeOfPartyReservationRequest()
        {
            return ReservationsApiClient.CreateChangeOfPartyReservation(_reservationId, _request, CancellationToken.None);
        }

        public void AssertUriCorrectlyFormed()
        {
            var expectedUrl = $"{Config.ApiBaseUrl}/api/reservations/{_reservationId}/change";

            HttpHelper.Verify(x => x.PostAsJson<CreateChangeOfPartyReservationRequest, CreateChangeOfPartyReservationResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<CreateChangeOfPartyReservationRequest>(), It.IsAny<CancellationToken>()));
        }

        public void AssertPayloadIsPassedInCorrectly()
        {
            HttpHelper.Verify(x => x.PostAsJson<CreateChangeOfPartyReservationRequest, CreateChangeOfPartyReservationResult>(It.IsAny<string>(), _request, It.IsAny<CancellationToken>()));
        }
    }
}
