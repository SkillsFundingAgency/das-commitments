﻿using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.UnitOfWork.Context;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class UpdateApprenticeshipStopDateCommandHandlerTests
    {
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<ILogger<UpdateApprenticeshipStopDateCommandHandler>> _logger;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IMessageSession> _nserviceBusContext;
        private Mock<IEncodingService> _encodingService;
        private Mock<IOverlapCheckService> _overlapCheckService;
        private ProviderCommitmentsDbContext _dbContext;
        private ProviderCommitmentsDbContext _confirmationDbContext;
        private UnitOfWorkContext _unitOfWorkContext { get; set; }
        private IRequestHandler<UpdateApprenticeshipStopDateCommand> _handler;
        private static CommitmentsV2Configuration commitmentsV2Configuration;
        private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;

        private readonly string ProviderCommitmentsBaseUrl = "https://approvals.ResourceEnvironmentName-pas.apprenticeships.education.gov.uk/";

        [SetUp]
        public void Init()
        {
            var databaseGuid = Guid.NewGuid().ToString();
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                        .UseInMemoryDatabase(databaseGuid, b => b.EnableNullChecks(false))
                                        .Options);

            _confirmationDbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                            .UseInMemoryDatabase(databaseGuid, b => b.EnableNullChecks(false))
                            .Options);

            _currentDateTime = new Mock<ICurrentDateTime>();
            _authenticationService = new Mock<IAuthenticationService>();
            _nserviceBusContext = new Mock<IMessageSession>();
            _encodingService = new Mock<IEncodingService>();
            _overlapCheckService = new Mock<IOverlapCheckService>();
            _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();

            _resolveOverlappingTrainingDateRequestService
                .Setup(x => x.Resolve(It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<Types.OverlappingTrainingDateRequestResolutionType>()))
                .Returns(Task.CompletedTask);

            _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()));
            _logger = new Mock<ILogger<UpdateApprenticeshipStopDateCommandHandler>>();
            _unitOfWorkContext = new UnitOfWorkContext();
            commitmentsV2Configuration = new CommitmentsV2Configuration()
            {
                ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl
            };

            _handler = new UpdateApprenticeshipStopDateCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
                _logger.Object,
                _currentDateTime.Object,
                _authenticationService.Object,
                 _nserviceBusContext.Object,
                _encodingService.Object,
                _overlapCheckService.Object,
                commitmentsV2Configuration,
                _resolveOverlappingTrainingDateRequestService.Object);
        }
        
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _confirmationDbContext?.Dispose();
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_WithInvalidData_ThenValidateErrorMessage()
        {
            // Arrange
            var command = new UpdateApprenticeshipStopDateCommand(0, 0, DateTime.MinValue, new UserInfo());
            UpdateApprenticeshipStopDateCommandValidator sut = new UpdateApprenticeshipStopDateCommandValidator();

            // Act
            var result = await sut.ValidateAsync(command, new CancellationToken());

            // Assert
            result.Errors.Should().SatisfyRespectively(
                first =>
                {
                    first.PropertyName.Should().Be("AccountId");
                    first.ErrorMessage.Should().Be("The Account Id must be positive");
                },
                second =>
                {
                    second.PropertyName.Should().Be("ApprenticeshipId");
                    second.ErrorMessage.Should().Be("The ApprenticeshipId must be positive");
                },
                third =>
                {
                    third.PropertyName.Should().Be("UserInfo");
                    third.ErrorMessage.Should().Be("The User Info supplied must not be null and contain a UserId");
                },
                fourth =>
                {
                    fourth.PropertyName.Should().Be("StopDate");
                    fourth.ErrorMessage.Should().Be("The StopDate must be supplied");
                });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WithInvalidCallingParty_ThenShouldThrowDomainException(UpdateApprenticeshipStopDateCommand command)
        {
            // Arrange
            await SetupApprenticeship(Party.Provider);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { ErrorMessage = "UpdateApprenticeshipStopDate is restricted to Employers only - Provider is invalid" });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_WithInvalidApprenticeshipForStop_PaymentStatusCompleted_ThenShouldThrowDomainException()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Completed);
            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "newStopDate", ErrorMessage = "Apprenticeship must be stopped in order to update stop date" });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_WhenValidatingApprenticeship_WithStopDateInFuture_ThenShouldThrowDomainException()
        {
            // Arrange
            var stopDate = DateTime.UtcNow.AddMonths(1);
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "newStopDate", ErrorMessage = "Invalid Date of Change. Date cannot be in the future." });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_WhenValidatingApprenticeship_WithStopDateBeforeStartDate_ThenShouldThrowDomainException()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn, startDate: DateTime.UtcNow.AddMonths(2));
            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "newStopDate", ErrorMessage = "The stop month cannot be before the apprenticeship started" });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_WithValidateEndDateOverlap_ThenShouldThrowDomainException()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            apprenticeship.Uln = "X";
            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, new UserInfo());
            _overlapCheckService.Setup(x => x.CheckForOverlaps(It.Is<string>(uln => uln == "X"), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(() => new OverlapCheckResult(false, true));

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "newStopDate", ErrorMessage = $"The date overlaps with existing dates for the same apprentice" });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_UpdateApprenticeshipStopDate_ThenShouldUpdateDatabaseRecord()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var newStopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, newStopDate, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work context transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var apprenticeshipAssertion = await _confirmationDbContext.Apprenticeships.FirstAsync(a => a.Id == apprenticeship.Id);
            apprenticeshipAssertion.StopDate.Should().Be(newStopDate);
            apprenticeshipAssertion.PaymentStatus.Should().Be(PaymentStatus.Withdrawn);
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_UpdateApprenticeshipStopDate_ThenShouldPublishApprenticeshipStoppedEvent()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Assert
            var stoppedEvent = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipStopDateChangedEvent>().First();

            stoppedEvent.Should().BeEquivalentTo(new ApprenticeshipStopDateChangedEvent
            {
                StopDate = stopDate,
                ApprenticeshipId = apprenticeship.Id,
                ChangedOn = _currentDateTime.Object.UtcNow
            });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_UpdateApprenticeshipStopDate_ThenShouldResolveDataLocks()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work context transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var dataLockAssertion = await _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id).ToListAsync();
            dataLockAssertion.Should().HaveCount(4);
            dataLockAssertion.Where(s => s.IsResolved).Should().HaveCount(2);
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_CreatesAddHistoryEvent()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work context transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var historyEvent = _unitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().First(e => e.EntityId == apprenticeship.Id);
            historyEvent.EntityType.Should().Be("Apprenticeship");
            historyEvent.StateChangeType.Should().Be(UserAction.UpdateApprenticeshipStopDate);
            var definition = new { StopDate = DateTime.MinValue, PaymentStatus = PaymentStatus.Withdrawn };
            var historyState = JsonConvert.DeserializeAnonymousType(historyEvent.UpdatedState, definition);

            historyState.StopDate.Should().Be(stopDate);
            historyState.PaymentStatus.Should().Be(PaymentStatus.Withdrawn);
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_UpdateApprenticeshipStopDate_ThenShouldSendProviderEmail(string hashedAppId)
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var fixture = new Fixture();
            apprenticeship.Cohort.ProviderId = fixture.Create<long>();
            var oldStopDate = apprenticeship.StopDate;
            _encodingService.Setup(a => a.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)).Returns(hashedAppId);
            var newStopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var templateName = "ProviderApprenticeshipStopEditNotification";
            var tokenUrl = $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{hashedAppId}";

            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, newStopDate, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Assert
            var tokens = new Dictionary<string, string>
            {
                {"EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name},
                {"APPRENTICE", apprenticeship.ApprenticeName },
                {"OLDDATE", oldStopDate.Value.ToString("dd/MM/yyyy") },
                {"NEWDATE", newStopDate.ToString("dd/MM/yyyy") },
                {"URL", tokenUrl }
            };

            _nserviceBusContext.Verify(s => s.Send(It.Is<SendEmailToProviderCommand>(x =>
                x.ProviderId == apprenticeship.Cohort.ProviderId &&
                x.Template == templateName &&
                VerifyTokens(x.Tokens, tokens)), It.IsAny<SendOptions>()));
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_UpdateApprenticeshipStopDate_ThenResolveOltd()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: PaymentStatus.Withdrawn);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new UpdateApprenticeshipStopDateCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work context transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            _resolveOverlappingTrainingDateRequestService
                .Verify(x => x.Resolve(It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<Types.OverlappingTrainingDateRequestResolutionType>()), Times.Once);
        }

        private static bool VerifyTokens(Dictionary<string, string> actualTokens, Dictionary<string, string> expectedTokens)
        {
            actualTokens.Should().BeEquivalentTo(expectedTokens);
            return true;
        }

        private async Task<Apprenticeship> SetupApprenticeship(Party party = Party.Employer, PaymentStatus paymentStatus = PaymentStatus.Active, DateTime? startDate = null)
        {
            var today = DateTime.UtcNow;
            _authenticationService.Setup(a => a.GetUserParty()).Returns(party);
            _currentDateTime.Setup(a => a.UtcNow).Returns(today);

            var fixture = new Fixture();
            var apprenticeshipId = fixture.Create<long>();
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = SetupDataLocks(apprenticeshipId),
                PaymentStatus = paymentStatus,
                StartDate = startDate != null ? startDate.Value : DateTime.UtcNow.AddMonths(-2),
                StopDate = DateTime.UtcNow.AddMonths(-1)
            };

            _dbContext.Apprenticeships.Add(apprenticeship);
            await _dbContext.SaveChangesAsync();

            return apprenticeship;
        }

        private static ICollection<DataLockStatus> SetupDataLocks(long apprenticeshipId)
        {
            var activeDataLock4 = new DataLockStatus
            {
                ApprenticeshipId = apprenticeshipId,
                EventStatus = EventStatus.New,
                IsExpired = false,
                TriageStatus = TriageStatus.Restart,
                ErrorCode = DataLockErrorCode.Dlock04
            };

            var activeDataLock5 = new DataLockStatus
            {
                ApprenticeshipId = apprenticeshipId,
                EventStatus = EventStatus.New,
                IsExpired = false,
                TriageStatus = TriageStatus.Restart,
                ErrorCode = DataLockErrorCode.Dlock05
            };

            var inactiveDataLock6 = new DataLockStatus
            {
                ApprenticeshipId = apprenticeshipId,
                EventStatus = EventStatus.Removed,
                IsExpired = false,
                TriageStatus = TriageStatus.Restart,
                ErrorCode = DataLockErrorCode.Dlock04
            };

            var dataLockForApprenticeshipBeforeStart = new DataLockStatus
            {
                ApprenticeshipId = apprenticeshipId,
                EventStatus = EventStatus.New,
                IsExpired = false,
                TriageStatus = TriageStatus.Change,
                ErrorCode = DataLockErrorCode.Dlock04
            };

            return new List<DataLockStatus> { activeDataLock4, activeDataLock5, inactiveDataLock6, dataLockForApprenticeshipBeforeStart };
        }
    }
}