using System;
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
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ResumeApprenticeshipCommandHandlerTests
    {
        public ProviderCommitmentsDbContext _dbContext;
        public Mock<IAuthenticationService> _authenticationService;
        public Mock<ICurrentDateTime> _currentDateTime;
        public IRequestHandler<ResumeApprenticeshipCommand> _handler;
        public UserInfo UserInfo { get; }
        private UnitOfWorkContext _unitOfWorkContext { get; set; }

        [SetUp]
        public void Init()
        {
            _authenticationService = new Mock<IAuthenticationService>();
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            _currentDateTime = new Mock<ICurrentDateTime>();
            _unitOfWorkContext = new UnitOfWorkContext();
            _handler = new ResumeApprenticeshipCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
                _currentDateTime.Object,
                _authenticationService.Object,
                Mock.Of<ILogger<ResumeApprenticeshipCommandHandler>>());
        }
        
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_ResumeApprenticeship_CreatesAddHistoryEvent()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship();

            var command = new ResumeApprenticeshipCommand
            {
                ApprenticeshipId = apprenticeship.Id,
                UserInfo = new UserInfo()
            };

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var historyEvent = _unitOfWorkContext.GetEvents().OfType<EntityStateChangedEvent>()
                .First(e => e.EntityId == apprenticeship.Id);
            historyEvent.EntityType.Should().Be("Apprenticeship");

            var definition = new { PaymentStatus = PaymentStatus.Paused };
            var historyState = JsonConvert.DeserializeAnonymousType(historyEvent.UpdatedState, definition);
            historyState.PaymentStatus.Should().Be(PaymentStatus.Active);
        }

        [Test]
        [InlineAutoData(PaymentStatus.Completed)]
        [InlineAutoData(PaymentStatus.Withdrawn)]
        [InlineAutoData(PaymentStatus.Active)]
        public async Task Handle_WhenHandlingCommand_ThrowErrorForNonPausedApprenticeshipStatus(PaymentStatus paymentStatus)
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(Party.Employer, paymentStatus);

            var command = new ResumeApprenticeshipCommand
            {
                ApprenticeshipId = apprenticeship.Id,
                UserInfo = new UserInfo()
            };

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { ErrorMessage = "Only paused record can be activated" });
        }

        [Test]
        [InlineAutoData(Party.Provider)]
        [InlineAutoData(Party.TransferSender)]
        [InlineAutoData(Party.None)]
        public async Task Handle_WhenHandlingCommand_ThrowDomainExceptionIfPartyIsNotEmployer(Party party)
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(party);

            var command = new ResumeApprenticeshipCommand
            {
                ApprenticeshipId = apprenticeship.Id,
                UserInfo = new UserInfo()
            };

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { ErrorMessage = $"Only employers are allowed to edit the end of completed records - {party} is invalid" });
        }

        private async Task<Apprenticeship> SetupApprenticeship(Party party = Party.Employer, PaymentStatus paymentStatus = PaymentStatus.Paused, DateTime? startDate = null)
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
                PaymentStatus = paymentStatus,
                StartDate = startDate != null ? startDate.Value : DateTime.UtcNow.AddMonths(-2)
            };

            _dbContext.Apprenticeships.Add(apprenticeship);
            await _dbContext.SaveChangesAsync();

            return apprenticeship;
        }
    }

}