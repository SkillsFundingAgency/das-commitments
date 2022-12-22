using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class WhenCreatingBulkReservations
    {
        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new WhenCreatingBulkReservationsFixtures();
            await fixture.BulkCreateReservations();
            fixture.AssertUriCorrectlyFormed();
        }

        [Test]
        public async Task ThenTheRequestPayloadIsPassedInCorrectly()
        {
            var fixture = new WhenCreatingBulkReservationsFixtures();
            await fixture.BulkCreateReservations();
            fixture.AssertPayloadIsPassedInCorrectly();
        }
    }

    public class WhenCreatingBulkReservationsFixtures : ReservationsClientTestFixtures
    {
        private readonly BulkCreateReservationsRequest _request;
        private const long AccountLegalEntity = 1890;

        public WhenCreatingBulkReservationsFixtures()
        {
            HttpHelper.Setup(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(It.IsAny<string>(),
                    It.IsAny<BulkCreateReservationsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BulkCreateReservationsResult(new List<Guid>()));

            _request = AutoFixture.Create<BulkCreateReservationsRequest>();
        }

        public Task BulkCreateReservations()
        {
            return ReservationsApiClient.BulkCreateReservations(AccountLegalEntity, _request, CancellationToken.None);
        }

        public void AssertUriCorrectlyFormed()
        {
            var expectedUrl = $"{Config.ApiBaseUrl}/api/reservations/accounts/{AccountLegalEntity}/bulk-create";

            HttpHelper.Verify(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<BulkCreateReservationsRequest>(), It.IsAny<CancellationToken>()));
        }

        public void AssertPayloadIsPassedInCorrectly()
        {
            HttpHelper.Verify(x => x.PostAsJson<BulkCreateReservationsRequest, BulkCreateReservationsResult>(It.IsAny<string>(), _request, It.IsAny<CancellationToken>()));
        }
    }
}
