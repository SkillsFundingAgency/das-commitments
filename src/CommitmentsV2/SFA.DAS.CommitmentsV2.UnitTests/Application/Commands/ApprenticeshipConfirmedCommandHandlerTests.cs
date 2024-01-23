using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmed;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using Xunit.Extensions.AssertExtensions;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipConfirmedCommandHandlerTests
    {
        ApprenticeshipConfirmedCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipConfirmedCommandHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_WhenNoConfirmationStatusIsFoundForApprenticeship_ThenANewConfirmationStatusIsCreatedAsConfirmed()
        {
            var command = _fixture.DataFixture.Create<ApprenticeshipConfirmedCommand>();
            await _fixture.Handle(command);

            var status = _fixture.GetApprenticeshipConfirmationStatus(command.ApprenticeshipId);

            status.ShouldNotBeNull();
            status.CommitmentsApprovedOn.Should().Be(command.CommitmentsApprovedOn);
            status.ApprenticeshipConfirmedOn.Should().Be(command.ConfirmedOn);
            status.ConfirmationOverdueOn.ShouldBeNull();
            status.ConfirmationStatus.Should().Be(ConfirmationStatus.Confirmed);
        }

        [Test]
        public async Task Handle_WhenAConfirmationStatusIsFoundForApprenticeshipAndIsNotConfirmed_ThenUpdateIfThisChangeIsLater()
        {
            _fixture.SeedData();

            var command = _fixture.DataFixture.Build<ApprenticeshipConfirmedCommand>()
                .With(x=> x.ApprenticeshipId, _fixture.ConfirmationStatusUnconfirmed.ApprenticeshipId)
                .With(x=> x.CommitmentsApprovedOn, _fixture.ConfirmationStatusUnconfirmed.CommitmentsApprovedOn.AddDays(1))
                .Create();

            await _fixture.Handle(command);

            var status = _fixture.GetApprenticeshipConfirmationStatus(command.ApprenticeshipId);

            status.ShouldNotBeNull();
            status.CommitmentsApprovedOn.Should().Be(command.CommitmentsApprovedOn);
            status.ApprenticeshipConfirmedOn.Should().Be(command.ConfirmedOn);
            status.ConfirmationOverdueOn.Should().Be(_fixture.ConfirmationStatusUnconfirmed.ConfirmationOverdueOn);
            status.ConfirmationStatus.Should().Be(ConfirmationStatus.Confirmed);
        }

        [Test]
        public async Task Handle_WhenAConfirmationStatusIsFoundForApprenticeship_ThenIgnoreIfThisChangeIsEarlier()
        {
            _fixture.SeedData();

            var command = _fixture.DataFixture.Build<ApprenticeshipConfirmedCommand>()
                .With(x => x.ApprenticeshipId, _fixture.ConfirmationStatusConfirmed.ApprenticeshipId)
                .With(x => x.CommitmentsApprovedOn, _fixture.ConfirmationStatusConfirmed.CommitmentsApprovedOn.AddDays(-1))
                .Create();

            await _fixture.Handle(command);

            var status = _fixture.GetApprenticeshipConfirmationStatus(command.ApprenticeshipId);

            status.ShouldNotBeNull();
            status.CommitmentsApprovedOn.Should().Be(_fixture.ConfirmationStatusConfirmed.CommitmentsApprovedOn);
            status.ApprenticeshipConfirmedOn.Should().Be(_fixture.ConfirmationStatusConfirmed.ApprenticeshipConfirmedOn);
            status.ConfirmationOverdueOn.Should().Be(_fixture.ConfirmationStatusConfirmed.ConfirmationOverdueOn);
            status.ConfirmationStatus.Should().Be(ConfirmationStatus.Confirmed);
        }
    }

    public class ApprenticeshipConfirmedCommandHandlerTestsFixture : IDisposable
    {
        public Fixture DataFixture { get; set; }
        public ApprenticeshipConfirmationStatus ConfirmationStatusConfirmed { get; set; }
        public ApprenticeshipConfirmationStatus ConfirmationStatusUnconfirmed { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<ApprenticeshipConfirmedCommand> Handler { get; set; }

        public ApprenticeshipConfirmedCommandHandlerTestsFixture()
        {
            DataFixture = new Fixture();

            ConfirmationStatusConfirmed = DataFixture.Build<ApprenticeshipConfirmationStatus>().Without(x => x.Apprenticeship).Create();
            ConfirmationStatusUnconfirmed = DataFixture.Build<ApprenticeshipConfirmationStatus>()
                .Without(x => x.Apprenticeship)
                .Without(x => x.ApprenticeshipConfirmedOn).Create();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            Handler = new ApprenticeshipConfirmedCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle(ApprenticeshipConfirmedCommand command)
        {
            await Handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public ApprenticeshipConfirmedCommandHandlerTestsFixture SeedData()
        {
            Db.ApprenticeshipConfirmationStatus.Add(ConfirmationStatusConfirmed);
            Db.ApprenticeshipConfirmationStatus.Add(ConfirmationStatusUnconfirmed);
            Db.SaveChanges();
            return this;
        }

        public ApprenticeshipConfirmationStatus GetApprenticeshipConfirmationStatus(long apprenticeshipId)
        {
            var status = Db.ApprenticeshipConfirmationStatus.FirstOrDefault(x => x.ApprenticeshipId == apprenticeshipId);

            return status;
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}