using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.SendCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Testing.AutoFixture;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class StopApprenticeshipCommandHandlerTests
    {
        private Mock<ICurrentDateTime> _currentDateTime;
        private Mock<ILogger<StopApprenticeshipCommandHandler>> _logger;
        private Mock<IEventPublisher> _eventPublisher;
        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IMessageHandlerContext> _nserviceBusContext;
        private Mock<IEncodingService> _encodingService;
        ProviderCommitmentsDbContext _dbContext;
        ProviderCommitmentsDbContext _confirmationDbContext;
        private UnitOfWorkContext _unitOfWorkContext { get; set; }
        private IRequestHandler<StopApprenticeshipCommand> _handler;

        [SetUp]
        public void Init()
        {
            var databaseGuid = Guid.NewGuid().ToString();
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                        .UseInMemoryDatabase(databaseGuid)
                                        .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                        .Options);

            _confirmationDbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                            .UseInMemoryDatabase(databaseGuid)
                            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                            .Options);

            _currentDateTime = new Mock<ICurrentDateTime>();
            _eventPublisher = new Mock<IEventPublisher>();
            _authenticationService = new Mock<IAuthenticationService>();
            _nserviceBusContext = new Mock<IMessageHandlerContext>();
            _encodingService = new Mock<IEncodingService>();
            _logger = new Mock<ILogger<StopApprenticeshipCommandHandler>>();
            _unitOfWorkContext = new UnitOfWorkContext();

            _handler = new StopApprenticeshipCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
                _currentDateTime.Object,
                _eventPublisher.Object,
                _authenticationService.Object,
                _nserviceBusContext.Object,
                _encodingService.Object,
                _logger.Object);
        }

        [Test, MoqAutoData]
        public void Handle_WhenHandlingCommand_WithInvalidCallingParty_ThenShouldThrowDomainException(StopApprenticeshipCommand command)
        {
            // Arrange
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Types.Party.Provider);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { ErrorMessage = "StopApprenticeship is restricted to Employers only - Provider is invalid" });
        }

        [Test]
        [InlineAutoData(PaymentStatus.Completed)]
        [InlineAutoData(PaymentStatus.Withdrawn)]
        public async Task Handle_WhenHandlingCommand_WithInvalidApprenticeshipForStop_PaymentStatusCompleted_ThenShouldThrowDomainException(PaymentStatus paymentStatus)
        {
            // Arrange
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);
            var apprenticeship = await AddApprenticeship(paymentStatus);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, false, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { PropertyName = "PaymentStatus", ErrorMessage = "Apprenticeship must be Active or Paused. Unable to stop apprenticeship" });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WithMismatchedAccountId_ThenShouldThrowDomainException()
        {
            // Arrange
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);
            var apprenticeship = await AddApprenticeship();
            var incorrectAccountId = apprenticeship.Cohort.EmployerAccountId - 1;
            var command = new StopApprenticeshipCommand(incorrectAccountId, apprenticeship.Id, DateTime.UtcNow, false, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { PropertyName = "accountId", ErrorMessage = $"Employer {command.AccountId} not authorised to access commitment {apprenticeship.Cohort.Id}, expected employer {apprenticeship.Cohort.EmployerAccountId}" });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WithApprenticeshipWaitingToStart_WithStopDateNotEqualStartDate_ThenShouldThrowDomainException()
        {
            // Arrange
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);
            var apprenticeship = await AddApprenticeship(futureStartDate: true);
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, DateTime.UtcNow, false, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { PropertyName = "stopDate", ErrorMessage = $"Invalid stop date. Date should be value of start date if training has not started." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WhenValidatingApprenticeship_WithStopDateInFuture_ThenShouldThrowDomainException()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var stopDate = DateTime.UtcNow.AddMonths(1);
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);
            _currentDateTime.Setup(a => a.UtcNow).Returns(today);
            var apprenticeship = await AddApprenticeship();
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { PropertyName = "stopDate", ErrorMessage = $"Invalid Stop Date. Stop date cannot be in the future." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_WhenValidatingApprenticeship_WithStopDateInPast_ThenShouldThrowDomainException()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var stopDate = DateTime.UtcNow.AddMonths(-3);
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);
            _currentDateTime.Setup(a => a.UtcNow).Returns(today);
            var apprenticeship = await AddApprenticeship();
            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { PropertyName = "stopDate", ErrorMessage = $"Invalid Stop Date. Stop date cannot be before the apprenticeship has started." });
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenShouldUpdateDatabaseRecord()
        {
            // Arrange
            var apprenticeship = await SetupDefaultToPassValidation();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo());

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
            var apprenticeship = await SetupDefaultToPassValidation();
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Assert
            _eventPublisher.Verify(s => s.Publish(It.Is<ApprenticeshipStoppedEvent>(x =>
                x.AppliedOn == _currentDateTime.Object.UtcNow &&
                x.ApprenticeshipId == apprenticeship.Id &&
                x.StopDate == stopDate)));
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingCommand_StoppingApprenticeship_ThenShouldSendProviderEmail(string hashedAppId)
        {
            // Arrange
            var apprenticeship = await SetupDefaultToPassValidation();
            var fixture = new Fixture();
            apprenticeship.Cohort.ProviderId = fixture.Create<long>();
            _encodingService.Setup(a => a.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)).Returns(hashedAppId);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var templateName = "ProviderApprenticeshipStopNotification";
            var tokenUrl = $"{apprenticeship.Cohort.ProviderId}/apprentices/manage/{hashedAppId}/details";
            var tokens = new Dictionary<string, string>
            {
                {"EMPLOYER",apprenticeship.Cohort.AccountLegalEntity.Name },
                {"APPRENTICE", apprenticeship.ApprenticeName },
                {"DATE",stopDate.ToString("dd/MM/yyyy") },
                {"URL",tokenUrl },
            };

            var command = new StopApprenticeshipCommand(apprenticeship.Cohort.EmployerAccountId, apprenticeship.Id, stopDate, false, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Assert
            _nserviceBusContext.Verify(s => s.Send(It.Is<SendEmailToProviderCommand>(x =>
                x.ProviderId == apprenticeship.Cohort.ProviderId &&
                x.Template == templateName &&
                VerifyTokens(x.Tokens, tokens)), It.IsAny<SendOptions>()));
        }

        private bool VerifyTokens(Dictionary<string, string> actualTokens, Dictionary<string, string> expectedTokens)
        {
            actualTokens.Should().BeEquivalentTo(expectedTokens);
            return true;
        }


        // Need to make sure we test datalocks correctly in StopApprenticeship
        // Rule validation on mediator call
        // Stop = Start date apprenticeship on locks and successful in general

        private async Task<Apprenticeship> SetupDefaultToPassValidation()
        {
            var today = DateTime.UtcNow;
            _authenticationService.Setup(a => a.GetUserParty()).Returns(Party.Employer);
            _currentDateTime.Setup(a => a.UtcNow).Returns(today);
            return await AddApprenticeship();
        }

        private async Task<Apprenticeship> AddApprenticeship(PaymentStatus paymentStatus = PaymentStatus.Active, bool futureStartDate = false)
        {
            var fixture = new Fixture();
            var apprenticeship = new Apprenticeship
            {
                Id = fixture.Create<long>(),
                Cohort = new Cohort
                {
                    EmployerAccountId = fixture.Create<long>(),
                    AccountLegalEntity = new AccountLegalEntity()
                },
                DataLockStatus = new List<DataLockStatus>(),
                PaymentStatus = paymentStatus,
                StartDate = futureStartDate ? DateTime.UtcNow.AddMonths(2) : DateTime.UtcNow.AddMonths(-2)
            };

            _dbContext.Apprenticeships.Add(apprenticeship);
            await _dbContext.SaveChangesAsync();

            return apprenticeship;
        }
    }
}