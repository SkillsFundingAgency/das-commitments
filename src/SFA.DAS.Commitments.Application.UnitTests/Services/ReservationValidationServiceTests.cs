using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.Commitments.Application.UnitTests.Services
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ReservationValidationServiceTests
    {
        [Test]
        public void Constructor_ValidCall_ShouldNotThrowException()
        {
            var fixtures = new ReservationValidationServiceTestFixtures();

            fixtures.CreateService();
        }

        [Test]
        public void Constructor_NullReservationsClient_ShouldThrowNullArgumentException()
        {
            var fixtures = new ReservationValidationServiceTestFixtures();

            Assert.Throws<ArgumentNullException>(() => fixtures.CreateServiceWithNullClient());
        }

        [Test]
        public void Constructor_NullLogger_ShouldThrowNullArgumentException()
        {
            var fixtures = new ReservationValidationServiceTestFixtures();

            Assert.Throws<ArgumentNullException>(() => fixtures.CreateServiceWithNullLogger());
        }

        [Test]
        public async Task CheckReservation_WithReservationId_ShouldCallApi()
        {
            var reservationId = Guid.NewGuid();

            var fixtures = new ReservationValidationServiceTestFixtures()
                .WithValidStartDate()
                .WithReservationId(reservationId);

            await fixtures.CheckReservation();

            fixtures.ReservationsApiClientMock
                .Verify(rac => rac.ValidateReservation(
                    It.Is<ReservationValidationMessage>(msg => msg.ReservationId == reservationId), 
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task CheckReservation_WithoutReservationId_ShouldNotCallApi()
        {
            var fixtures = new ReservationValidationServiceTestFixtures();

            await fixtures.CheckReservation();

            fixtures.ReservationsApiClientMock
                .Verify(rac => rac.ValidateReservation(
                    It.IsAny<ReservationValidationMessage>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CheckReservation_WithValidationFailures_ShouldHaveHasErrorsTrue()
        {
            var reservationId = Guid.NewGuid();

            var fixtures = new ReservationValidationServiceTestFixtures()
                .WithValidStartDate()
                .WithReservationId(reservationId)
                .WithReservationError("startdate", "some error")
                .WithReservationError("coursecode", "some code");

            await fixtures.CheckReservation(result => Assert.IsTrue(result.HasErrors));
        }

        [Test]
        public async Task CheckReservation_WithValidationFailures_ShouldReturnExpectedErrors()
        {
            // arrange
            var reservationId = Guid.NewGuid();

            void AssertContains(ReservationValidationError[] errors, string propertyName, string reason)
            {
                var requiredEntry = errors.Where(e => e.PropertyName == propertyName && e.Reason == reason).ToArray();
                Assert.AreEqual(1, requiredEntry.Length, $"The returned errors contain {requiredEntry.Length} errors for property '{propertyName}' with reason '{reason}' when only one was expected.");
            }

            var fixtures = new ReservationValidationServiceTestFixtures()
                .WithValidStartDate()
                .WithReservationId(reservationId)
                .WithReservationError("startdate", "some date error")
                .WithReservationError("coursecode", "some course error");

            const int expectedNumberOfErrors = 2;

            // Act + Assert
            await fixtures.CheckReservation(result =>
            {
                Assert.AreEqual(expectedNumberOfErrors, result.ValidationErrors.Length);
                AssertContains(result.ValidationErrors, "startdate", "some date error");
                AssertContains(result.ValidationErrors, "coursecode", "some course error");
            });
        }
    }

    public class ReservationValidationServiceTestFixtures
    {
        private Guid? _reservationId;
        private DateTime? _startDate;
        private string _trainingCode;

        public ReservationValidationServiceTestFixtures()
        {
            ReservationsApiClientMock = new Mock<IReservationsApiClient>();    
            CommitmentsLoggerMock = new Mock<ICommitmentsLogger>();

            ReservationClientValidationResult = new ReservationValidationResult();

            ReservationsApiClientMock
                .Setup(rac =>
                    rac.ValidateReservation(It.IsAny<ReservationValidationMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ReservationClientValidationResult);
        }

        public Mock<IReservationsApiClient> ReservationsApiClientMock { get; }
        public IReservationsApiClient ReservationsApiClient => ReservationsApiClientMock.Object;

        public Mock<ICommitmentsLogger> CommitmentsLoggerMock { get; }
        public ICommitmentsLogger CommitmentsLogger => CommitmentsLoggerMock.Object;

        public ReservationValidationResult ReservationClientValidationResult { get; }

        public ReservationValidationService CreateService()
        {
            return new ReservationValidationService(ReservationsApiClient, CommitmentsLogger);
        }

        public ReservationValidationService CreateServiceWithNullClient()
        {
            return new ReservationValidationService(null, CommitmentsLogger);
        }

        public ReservationValidationService CreateServiceWithNullLogger()
        {
            return new ReservationValidationService(ReservationsApiClient, null);
        }

        public ReservationValidationServiceTestFixtures WithReservationId(Guid reservationId)
        {
            _reservationId = reservationId;
            return this;
        }

        public ReservationValidationServiceTestFixtures WithReservationError(string propertyName, string reason)
        {
            var errors = ReservationClientValidationResult.ValidationErrors;
            var newSize = errors.Length + 1;
            Array.Resize(ref errors, newSize);
            errors[newSize-1] = new ReservationValidationError { PropertyName = propertyName, Reason = reason};
            ReservationClientValidationResult.ValidationErrors = errors;

            return this;
        }

        public ReservationValidationServiceTestFixtures WithValidStartDate()
        {
            return WithStartDate(DateTime.Today);
        }

        public ReservationValidationServiceTestFixtures WithStartDate(DateTime? startDate)
        {
            _startDate = startDate;
            return this;
        }

        public ReservationValidationServiceTestFixtures WithTrainingCode(string trainingCode)
        {
            _trainingCode = trainingCode;
            return this;
        }

        public Task CheckReservation()
        {
            return CheckReservation(null);
        }

        public async Task CheckReservation(Action<ReservationValidationResult> checker)
        {
            var service = CreateService();

            var request = new ReservationValidationServiceRequest
            {
                ReservationId = _reservationId,
                StartDate = _startDate,
                TrainingCode = _trainingCode
            };

            var result = await service.CheckReservation(request);

            checker?.Invoke(result);
        }
    }
}
