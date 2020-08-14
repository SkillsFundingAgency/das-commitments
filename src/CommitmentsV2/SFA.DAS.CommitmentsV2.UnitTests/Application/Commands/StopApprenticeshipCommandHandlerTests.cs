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
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Testing.AutoFixture;

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
        private IRequestHandler<StopApprenticeshipCommand> _handler;

        [SetUp]
        public void Init()
        {
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                        .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                        .Options);

            _currentDateTime = new Mock<ICurrentDateTime>();
            _eventPublisher = new Mock<IEventPublisher>();
            _authenticationService = new Mock<IAuthenticationService>();
            _nserviceBusContext = new Mock<IMessageHandlerContext>();
            _encodingService = new Mock<IEncodingService>();
            _logger = new Mock<ILogger<StopApprenticeshipCommandHandler>>();

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


        // Need to make sure we test data locks correctly in StopApprenticeship

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