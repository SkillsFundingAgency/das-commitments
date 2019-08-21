using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client.UnitTests
{
    [TestFixture]
    [Parallelizable]
    public class WhenCallingTheBulkCreateReservationsEndpoint
    {
        private ReservationsClientTestFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new ReservationsClientTestFixture();   
        }

        [Test]
        public async Task ThenTheBulkCreateReservationsRequestUriIsCorrectlyFormed()
        {
            var request = new BulkCreateReservationsRequest{ Count = 100 };
            var accountLegalEntityId = 1234;

            await _fixture.BulkCreateReservationsRequest(accountLegalEntityId, request);
            _fixture.AssertBulkCreateReservationsUriCorrectlyFormed(accountLegalEntityId);
        }

        [Test]
        public async Task ThenTheBulkCreateReservationsRequestPayloadIsCorrectlyFormed()
        {
            var request = new BulkCreateReservationsRequest { Count = 100 };
            var accountLegalEntityId = 1234;

            await _fixture.BulkCreateReservationsRequest(accountLegalEntityId, request);
            _fixture.AssertBulkCreateReservationsPayloadCorrectlyPassedToMethod(request);
        }
    }
}