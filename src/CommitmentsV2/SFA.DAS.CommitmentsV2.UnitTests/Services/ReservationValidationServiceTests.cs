using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.Reservations.Api.Types;
using System.Threading;
using System.Threading.Tasks;
using ReservationValidationResult = SFA.DAS.Reservations.Api.Types.ReservationValidationResult;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ReservationValidationServiceTests
    {
        private Fixture _fixture;
        private ReservationValidationService _reservationValidationService;
        private Mock<IMapper<ReservationValidationResult, CommitmentsV2.Domain.Entities.Reservations.ReservationValidationResult>> _resultMapper;
        private Mock<IMapper<ReservationValidationRequest, ValidationReservationMessage>> _requestMapper;
        private CommitmentsV2.Domain.Entities.Reservations.ReservationValidationResult _validationResult;
        private ReservationValidationRequest _validationRequest;
        private Mock<IReservationsApiClient> _apiClient;
        private ValidationReservationMessage _apiRequest;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _validationResult = _fixture.Create<CommitmentsV2.Domain.Entities.Reservations.ReservationValidationResult>();
            _validationRequest = _fixture.Create<ReservationValidationRequest>();

            _apiRequest = new ValidationReservationMessage();

            _requestMapper = new Mock<IMapper<ReservationValidationRequest, ValidationReservationMessage>>();
            _requestMapper.Setup(x => x.Map(It.Is<ReservationValidationRequest>(r => r == _validationRequest)))
                .ReturnsAsync(() => _apiRequest);

            _resultMapper = new Mock<IMapper<ReservationValidationResult, CommitmentsV2.Domain.Entities.Reservations.ReservationValidationResult>>();
            _resultMapper.Setup(x => x.Map(It.IsAny<ReservationValidationResult>())).ReturnsAsync(_validationResult);

            _apiClient = new Mock<IReservationsApiClient>();
            _apiClient.Setup(x =>
                    x.ValidateReservation(It.Is<ValidationReservationMessage>(r => r == _apiRequest), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReservationValidationResult());

                _reservationValidationService =
                    new ReservationValidationService(_apiClient.Object, _requestMapper.Object, _resultMapper.Object);
        }

        [Test]
        public async Task ValidationResultIsReturned()
        {
            var result = await _reservationValidationService.Validate(_validationRequest, new CancellationToken());
            Assert.AreEqual(_validationResult, result);
        }
    }
}
