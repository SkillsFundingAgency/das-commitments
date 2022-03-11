using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Models.Api.Types;
using SFA.DAS.CommitmentsV2.TestHelpers;
using Xunit.Extensions.AssertExtensions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipEmailAddressChangedByApprenticeCommandHandlerTests
    {
        ApprenticeshipEmailAddressChangedByApprenticeCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipEmailAddressChangedByApprenticeCommandHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsStopped_ThenEmailAddressDoesNotGetUpdated()
        {
            var command = _fixture.ApprenticeshipEmailAddressChangedByApprenticeCommand;
            _fixture.SeedData(true);
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.Email.Should().Be(_fixture.CurrentEmailAddress);
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsCompleted_ThenEmailAddressDoesNotChange()
        {
            var command = _fixture.ApprenticeshipEmailAddressChangedByApprenticeCommand;
            _fixture.SeedData(completed: true);
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.Email.Should().Be(_fixture.CurrentEmailAddress);
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsNotStoppedOrCompleted_ThenEmailAddressDoesGetUpdated()
        {
            var command = _fixture.ApprenticeshipEmailAddressChangedByApprenticeCommand;
            _fixture.SeedData();
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.EmailAddressConfirmed.Should().BeTrue();
            apprenticeship.Email.Should().Be(_fixture.NewEmailAddress);
        }

        [Test]
        public async Task Handle_WhenApprenticeshipEmailAddressIsNotConfirmed_ThenThrowException()
        {
            var command = _fixture.ApprenticeshipEmailAddressChangedByApprenticeCommand;
            _fixture.SeedData(emailAddressConfirmed:null);

            try
            {
                await _fixture.Handle(command);
                Assert.Fail("Error should have been thrown");
            }
            catch (DomainException e)
            {
                var errors = e.DomainErrors.ToList();
                errors.Count.Should().Be(1);
                errors[0].PropertyName.Should().Be("Email");
                errors[0].ErrorMessage.ShouldStartWith("Email Address cannot be updated for");
            }
        }
    }

    public class ApprenticeshipEmailAddressChangedByApprenticeCommandHandlerTestsFixture
    {
        public long ApprenticeshipId = 12;
        public Fixture DataFixture { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<ApprenticeshipEmailAddressChangedByApprenticeCommand> Handler { get; set; }
        public Mock<IApprovalsOuterApiClient> ApiClientMock { get; set; }
        public string CurrentEmailAddress { get; set; }
        public string NewEmailAddress { get; set; }
        public ApprenticeResponse ApprenticeResponse { get; set; }
        public ApprenticeshipEmailAddressChangedByApprenticeCommand ApprenticeshipEmailAddressChangedByApprenticeCommand { get; set; }

        public ApprenticeshipEmailAddressChangedByApprenticeCommandHandlerTestsFixture()
        {
            DataFixture = new Fixture();
            CurrentEmailAddress = "initial@Email.com";
            CurrentEmailAddress = "new@Email.com";
            ApprenticeResponse = new ApprenticeResponse {Email = NewEmailAddress };
            ApprenticeshipEmailAddressChangedByApprenticeCommand =
                DataFixture.Create<ApprenticeshipEmailAddressChangedByApprenticeCommand>();

            ApiClientMock = new Mock<IApprovalsOuterApiClient>();
            ApiClientMock.Setup(x => x.Get<ApprenticeResponse>(It.IsAny<GetApprenticeRequest>()))
                .ReturnsAsync(ApprenticeResponse);

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                .Options);

            Handler = new ApprenticeshipEmailAddressChangedByApprenticeCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), ApiClientMock.Object,
                Mock.Of<ILogger<ApprenticeshipEmailAddressChangedByApprenticeCommandHandler>>());
        }

        public async Task Handle(ApprenticeshipEmailAddressChangedByApprenticeCommand command)
        {
            await Handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public ApprenticeshipEmailAddressChangedByApprenticeCommandHandlerTestsFixture SeedData(bool stopped = false, bool completed = false, bool? emailAddressConfirmed = true)
        {
            var apprenticeship = new Apprenticeship();
            apprenticeship.SetValue(x=>x.Id, ApprenticeshipEmailAddressChangedByApprenticeCommand.ApprenticeshipId);
            apprenticeship.SetValue(x => x.FirstName, "FirstName");
            apprenticeship.SetValue(x => x.LastName, "LastName");
            apprenticeship.SetValue(x => x.Email, CurrentEmailAddress);
            apprenticeship.SetValue(x=>x.EmailAddressConfirmed, emailAddressConfirmed);
            if (stopped)
            {
                apprenticeship.SetValue(x => x.StopDate, DateTime.Now);
                apprenticeship.SetValue(x=>x.PaymentStatus, PaymentStatus.Withdrawn);
            }

            if (completed)
            {
                apprenticeship.SetValue(x => x.CompletionDate, DateTime.Now);
                apprenticeship.SetValue(x => x.PaymentStatus, PaymentStatus.Completed);
            }

            apprenticeship.SetValue(x => x.Cohort, new Cohort
            {
                AccountLegalEntity = new AccountLegalEntity(),
                ProviderId = 1
            });

            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();
            return this;
        }

        public Apprenticeship GetApprenticeship(long apprenticeshipId) 
            => Db.Apprenticeships.FirstOrDefault(x => x.Id == apprenticeshipId);
    }
}