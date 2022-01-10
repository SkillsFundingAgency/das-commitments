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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation;
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
    public class ResendInvitationCommandHandlerTests
    {
        public ProviderCommitmentsDbContext _dbContext;
        public Mock<IAuthenticationService> _authenticationService;
        public Mock<ICurrentDateTime> _currentDateTime;
        public IRequestHandler<ResendInvitationCommand> _handler;
        public UserInfo UserInfo { get; }
        private UnitOfWorkContext _unitOfWorkContext { get; set; }

        [SetUp]
        public void Init()
        {
            _authenticationService = new Mock<IAuthenticationService>();
            _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            _currentDateTime = new Mock<ICurrentDateTime>();
            _unitOfWorkContext = new UnitOfWorkContext();
            _handler = new ResendInvitationCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => _dbContext),
                _currentDateTime.Object,
                _authenticationService.Object,
                Mock.Of<ILogger<ResendInvitationCommandHandler>>());
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public async Task Handle_WhenHandlingCommand_ResendInvitation_CreatesAddHistoryEvent(Party party)
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(party, "test@test.com");

            var command = new ResendInvitationCommand(apprenticeship.Id, new UserInfo());

            // Act
            await _handler.Handle(command, new CancellationToken());

            // Simulate Unit of Work contex transaction ending in http request.
            await _dbContext.SaveChangesAsync();

            // Assert
            var @event = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipResendInvitationEvent>()
                .First(e => e.ApprenticeshipId == apprenticeship.Id);
            @event.ApprenticeshipId.Should().Be(apprenticeship.Id);
            @event.ResendOn.Should().Be(_currentDateTime.Object.UtcNow);
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_ThrowErrorWhenNoEmailAddress()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(email:null);

            var command = new ResendInvitationCommand(apprenticeship.Id, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { ErrorMessage = "Invitation cannot be sent as there is no email associated with apprenticeship" });
        }

        [Test]
        public async Task Handle_WhenHandlingCommand_ThrowErrorWhenEmailAddressHasBeenConfirmed()
        {
            // Arrange
            var apprenticeship = await SetupApprenticeship(email: "test@test.com", emailAddressConfirmed: true);

            var command = new ResendInvitationCommand(apprenticeship.Id, new UserInfo());

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, new CancellationToken()));

            // Assert
            exception.DomainErrors.Should().BeEquivalentTo(new { ErrorMessage = "Email address has been confirmed" });
        }

        private async Task<Apprenticeship> SetupApprenticeship(Party party = Party.Employer, string email = null, bool? emailAddressConfirmed = null)
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
                PaymentStatus = PaymentStatus.Active,
                StartDate = DateTime.UtcNow.AddMonths(-2), 
                Email = email,
                EmailAddressConfirmed = emailAddressConfirmed
            };

            _dbContext.Apprenticeships.Add(apprenticeship);
            await _dbContext.SaveChangesAsync();

            return apprenticeship;
        }
    }
}