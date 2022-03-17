using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.TestHelpers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Extensions.AssertExtensions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipEmailAddressConfirmedCommandHandlerTests
    {
        ApprenticeshipEmailAddressConfirmedCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipEmailAddressConfirmedCommandHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsStopped_ThenEmailAddressDoesNotGetConfirmed()
        {
            var command = _fixture.ApprenticeshipEmailAddressConfirmedCommand;
            _fixture.SeedData(true);
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.EmailAddressConfirmed.Should().BeNull();
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsCompleted_ThenEmailAddressDoesNotGetConfirmed()
        {
            var command = _fixture.ApprenticeshipEmailAddressConfirmedCommand;
            _fixture.SeedData(completed: true);
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.EmailAddressConfirmed.Should().BeNull();
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsNotStoppedOrCompleted_ThenEmailAddressDoesGetConfirmedAndUpdated()
        {
            var command = _fixture.ApprenticeshipEmailAddressConfirmedCommand;
            _fixture.ApprenticeResponse.Email = "NewEmail@test.com";
            _fixture.SeedData();
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.EmailAddressConfirmed.Should().BeTrue();
            apprenticeship.Email.Should().Be(_fixture.ApprenticeResponse.Email);
        }

        [Test]
        public async Task Handle_WhenApprenticeshipIsAlreadyConfirmed_ThenEmailAddressDoesGetUpdated()
        {
            var command = _fixture.ApprenticeshipEmailAddressConfirmedCommand;
            _fixture.ApprenticeResponse.Email = "NewEmail@test.com";
            _fixture.SeedData(emailAddressConfirmed:true);
            await _fixture.Handle(command);

            var apprenticeship = _fixture.GetApprenticeship(command.ApprenticeshipId);

            apprenticeship.ShouldNotBeNull();
            apprenticeship.EmailAddressConfirmed.Should().BeTrue();
            apprenticeship.Email.Should().Be(_fixture.CurrentEmailAddress);
        }
    }

    public class ApprenticeshipEmailAddressConfirmedCommandHandlerTestsFixture
    {
        public long ApprenticeshipId = 12;
        public Fixture DataFixture { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<ApprenticeshipEmailAddressConfirmedCommand> Handler { get; set; }
        public Mock<IApprovalsOuterApiClient> ApiClientMock { get; set; }
        public string CurrentEmailAddress { get; set; }
        public ApprenticeResponse ApprenticeResponse { get; set; }
        public ApprenticeshipEmailAddressConfirmedCommand ApprenticeshipEmailAddressConfirmedCommand { get; set; }

        public ApprenticeshipEmailAddressConfirmedCommandHandlerTestsFixture()
        {
            DataFixture = new Fixture();
            CurrentEmailAddress = "initial@Email.com";
            ApprenticeResponse = new ApprenticeResponse {Email = CurrentEmailAddress };
            ApprenticeshipEmailAddressConfirmedCommand =
                DataFixture.Create<ApprenticeshipEmailAddressConfirmedCommand>();

            ApiClientMock = new Mock<IApprovalsOuterApiClient>();
            ApiClientMock.Setup(x => x.Get<ApprenticeResponse>(It.IsAny<GetApprenticeRequest>()))
                .ReturnsAsync(ApprenticeResponse);

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            Handler = new ApprenticeshipEmailAddressConfirmedCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), ApiClientMock.Object,
                Mock.Of<ILogger<ApprenticeshipEmailAddressConfirmedCommandHandler>>());
        }

        public async Task Handle(ApprenticeshipEmailAddressConfirmedCommand command)
        {
            await Handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public ApprenticeshipEmailAddressConfirmedCommandHandlerTestsFixture SeedData(bool stopped = false, bool completed = false, bool? emailAddressConfirmed = null)
        {
            var apprenticeship = new Apprenticeship();
            apprenticeship.SetValue(x=>x.Id, ApprenticeshipEmailAddressConfirmedCommand.ApprenticeshipId);
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