using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.TestHelpers;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class WhenRetrievingStatus
    {
        [Test]
        public async Task ThenTheRequestUriIsCorrectlyFormed()
        {
            var fixture = new WhenRetrievingStatusTestFixtures();
            await fixture.ValidateReservation();
            fixture.AssertUriCorrectlyFormed();
        }

        [Test]
        public async Task ThenTheRequestPayloadIsCorrectlyFormed()
        {
            var fixture = new WhenRetrievingStatusTestFixtures();
            await fixture.ValidateReservation();
            fixture.AssertPayloadCorrectlyFormed();
        }
    }

    public class WhenRetrievingStatusTestFixtures : ReservationsClientTestFixtures
    {
        private readonly ReservationAllocationStatusMessage _request;

        public WhenRetrievingStatusTestFixtures()
        {
            HttpHelper.Setup(x => x.GetAsync<ReservationAllocationStatusResult>(It.IsAny<string>(),
                    null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReservationAllocationStatusResult());

            _request = AutoFixture.Create<ReservationAllocationStatusMessage>();
        }

        public Task ValidateReservation()
        {
            return ReservationsApiClient.GetReservationAllocationStatus(_request, new CancellationToken());
        }

        public void AssertUriCorrectlyFormed()
        {
            var expectedUrl = $"{Config.ApiBaseUrl}/api/accounts/{_request.AccountId}/status";

            HttpHelper.Verify(x => x.GetAsync<ReservationAllocationStatusResult>(It.Is<string>(actualUrl => IsSameUri(expectedUrl, actualUrl)),
                It.IsAny<object>(), It.IsAny<CancellationToken>()));
        }

        public void AssertPayloadCorrectlyFormed()
        {
            var expectedPayload = new
            {
                StartDate = _request.AccountId
            };

            HttpHelper.Verify(x => x.GetAsync<ReservationAllocationStatusResult>(It.IsAny<string>(), null, It.IsAny<CancellationToken>()));
        }
    }
}
