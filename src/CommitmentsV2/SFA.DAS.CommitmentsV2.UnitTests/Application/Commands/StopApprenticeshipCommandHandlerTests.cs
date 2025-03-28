using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.UnitOfWork.Context;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class StopApprenticeshipCommandHandlerTests
    {
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<ILogger<StopApprenticeshipCommandHandler>> _logger;
        private Mock<IMessageSession> _nserviceBusContext;
        private Mock<IEncodingService> _encodingService;
        private ProviderCommitmentsDbContext _dbContext;
        private ProviderCommitmentsDbContext _confirmationDbContext;
        private UnitOfWorkContext _unitOfWorkContext { get; set; }
        private IRequestHandler<StopApprenticeshipCommand> _handler;
        private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
        private const string ProviderCommitmentsBaseUrl = "https://approvals";

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
            _nserviceBusContext = new Mock<IMessageSession>();
            _encodingService = new Mock<IEncodingService>();
            _logger = new Mock<ILogger<StopApprenticeshipCommandHandler>>();
            _unitOfWorkContext = new UnitOfWorkContext();

            _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
            _resolveOverlappingTrainingDateRequestService
             .Setup(x => x.Resolve(It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<Types.OverlappingTrainingDateRequestResolutionType>()))
             .Returns(Task.CompletedTask);

            _handler = new StopApprenticeshipCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
                _currentDateTime.Object,
                _nserviceBusContext.Object,
                _encodingService.Object,
                _logger.Object,
                new CommitmentsV2Configuration { ProviderCommitmentsBaseUrl = ProviderCommitmentsBaseUrl },
                _resolveOverlappingTrainingDateRequestService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _confirmationDbContext?.Dispose();
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_WithInvalidData_ThenShouldThrowException()
        {
            // Arrange
            var command = new StopApprenticeshipCommand(0, 0, DateTime.MinValue, false, new UserInfo(), Party.None);
            StopApprenticeshipCommandValidator sut = new StopApprenticeshipCommandValidator();

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
                },
                fifth =>
                {
                    fifth.PropertyName.Should().Be("Party");
                    fifth.ErrorMessage.Should().Be("The Party must be supplied");
                });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WithInvalidCallingParty_ThenShouldThrowDomainException()
        {
            // Arrange
            await SetupApprenticeship();

            var fixture = new Fixture();
            var command = fixture.Build<StopApprenticeshipCommand>()
                .With(x => x.Party, Party.Provider)
                .Create();
            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { ErrorMessage = "StopApprenticeship is restricted to Employers only - Provider is invalid" });
        }

        [Test]
        [InlineAutoData(PaymentStatus.Completed)]
        [InlineAutoData(PaymentStatus.Withdrawn)]
        public async Task Handle_WhenHandlingCommand_WithInvalidApprenticeshipForStop_PaymentStatusCompleted_ThenShouldThrowDomainException(PaymentStatus paymentStatus)
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(paymentStatus: paymentStatus);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, false, new UserInfo(), Party.Employer);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "PaymentStatus", ErrorMessage = "Apprenticeship must be Active or Paused. Unable to stop apprenticeship" });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WithMismatchedAccountId_ThenShouldThrowDomainException()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var incorrectAccountId = apprenticeship.Cohort.EmployerAccountId - 1;
            var command = new StopApprenticeshipCommand(incorrectAccountId, apprenticeship.Id, DateTime.UtcNow, false, new UserInfo(), Party.Employer);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "accountId", ErrorMessage = $"Employer {command.AccountId} not authorised to access commitment {apprenticeship.Cohort.Id}, expected employer {apprenticeship.Cohort.EmployerAccountId}" });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WithApprenticeshipWaitingToStart_WithStopDateNotEqualStartDate_ThenShouldThrowDomainException()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(startDate: DateTime.UtcNow.AddMonths(2));
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, false, new UserInfo(), Party.Employer);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = $"Invalid stop date. Date should be value of start date if training has not started." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WhenValidatingApprenticeship_WithStopDateInFuture_ThenShouldThrowDomainException()
        {
            // Arrange
            var stopDate = DateTime.UtcNow.AddMonths(1);
            var apprenticeship = await SetupApprenticeship();
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = $"Invalid Stop Date. Stop date cannot be in the future and must be the 1st of the month." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WhenValidatingSimplifiedPaymentsApprenticeship_WithStopDateInFuture_ThenShouldThrowDomainException()
        {
            // Arrange
            var stopDate = DateTime.UtcNow.AddDays(1);
            var apprenticeship = await SetupApprenticeship(isOnFlexiPaymentsPilot: true);
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = $"Invalid Stop Date. Stop date cannot be in the future." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WhenValidatingApprenticeship_WithStopDateInPast_ThenShouldThrowDomainException()
        {
            // Arrange
            var stopDate = DateTime.UtcNow.AddMonths(-3);
            var apprenticeship = await SetupApprenticeship();
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = $"Invalid Stop Date. Stop date cannot be before the apprenticeship has started." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenShouldUpdateDatabaseRecord()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var apprenticeshipAssertion = await _confirmationDbContext.Apprenticeships.FirstAsync(a => a.Id == apprenticeship.Id);
            apprenticeshipAssertion.StopDate.Should().Be(stopDate);
            apprenticeshipAssertion.MadeRedundant.Should().Be(false);
            apprenticeshipAssertion.PaymentStatus.Should().Be(PaymentStatus.Withdrawn);
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenShouldPublishApprenticeshipStoppedEvent()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Assert
            var stoppedEvent = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipStoppedEvent>().First();

            stoppedEvent.Should().BeEquivalentTo(new ApprenticeshipStoppedEvent
            {
                AppliedOn = _currentDateTime.Object.UtcNow,
                ApprenticeshipId = apprenticeship.Id,
                StopDate = stopDate
            });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenShouldSendProviderEmail(string hashedAppId)
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var fixture = new Fixture();
            apprenticeship.Cohort.ProviderId = fixture.Create<long>();
            _encodingService.Setup(a => a.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)).Returns(hashedAppId);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var templateName = "ProviderApprenticeshipStopNotification";
            var tokenUrl = $"{ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{hashedAppId}";

            var tokens = new Dictionary<string, string>
            {
                {"EMPLOYER",apprenticeship.Cohort.AccountLegalEntity.Name },
                {"APPRENTICE", apprenticeship.ApprenticeName },
                {"DATE",stopDate.ToString("dd/MM/yyyy") },
                {"URL",tokenUrl },
            };

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Assert
            _nserviceBusContext.Verify(s => s.Send(It.Is<SendEmailToProviderCommand>(x =>
                x.ProviderId == apprenticeship.Cohort.ProviderId &&
                x.Template == templateName &&
                VerifyTokens(x.Tokens, tokens)), It.IsAny<SendOptions>()));
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenShouldResolveDataLocks()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var dataLockAssertion = await _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id).ToListAsync();
            dataLockAssertion.Should().HaveCount(4);
            dataLockAssertion.Where(s => s.IsResolved).Should().HaveCount(2);
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeshipBeforeItSarts_ThenShouldResolveDataLocks()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddMonths(4);
            var startAndStopDate = new DateTime(futureDate.Year, futureDate.Month, 1);

            var apprenticeship = await SetupApprenticeship(startDate: startAndStopDate);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, startAndStopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var dataLockAssertion = await _confirmationDbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id).ToListAsync();
            dataLockAssertion.Should().HaveCount(4);
            dataLockAssertion.Where(s => s.IsResolved).Should().HaveCount(3);
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_CreatesAddHistoyEvent()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var historyEvent = _unitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>().First(e => e.EntityId == apprenticeship.Id);
            historyEvent.EntityType.Should().Be("Apprenticeship");
            historyEvent.StateChangeType.Should().Be(UserAction.StopApprenticeship);
            var definition = new { StopDate = DateTime.MinValue, MadeRedundant = true, PaymentStatus = PaymentStatus.Active };
            var historyState = JsonConvert.DeserializeAnonymousType(historyEvent.UpdatedState, definition);

            historyState.StopDate.Should().Be(stopDate);
            historyState.MadeRedundant.Should().Be(false);
            historyState.PaymentStatus.Should().Be(PaymentStatus.Withdrawn);
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenResolveOltd()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo(), Party.Employer);

            // Act
            await _handler.Handle(command, new CancellationToken());
            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            _resolveOverlappingTrainingDateRequestService
                .Verify(x => x.Resolve(It.IsAny<long?>(), It.IsAny<long?>(), Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped), Times.Once);
        }

        private static bool VerifyTokens(IDictionary<string, string> actualTokens, Dictionary<string, string> expectedTokens)
        {
            actualTokens.Should().BeEquivalentTo(expectedTokens);
            return true;
        }

        private async Task<Apprenticeship> SetupApprenticeship(PaymentStatus paymentStatus = PaymentStatus.Active, DateTime? startDate = null, bool isOnFlexiPaymentsPilot = false)
        {
            var today = DateTime.UtcNow;
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
                IsOnFlexiPaymentPilot = isOnFlexiPaymentsPilot
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