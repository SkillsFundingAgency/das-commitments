using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipConfirmationCommenced;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipConfirmationCommencedCommandHandlerTests
    {
        ApprenticeshipConfirmationCommencedCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipConfirmationCommencedCommandHandlerTestsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_WhenNoConfirmationStatusIsFoundForApprenticeship_ThenANewConfirmationStatusIsCreatedAsUnconfirmed()
        {
            var command = _fixture.DataFixture.Create<ApprenticeshipConfirmationCommencedCommand>();
            await _fixture.Handle(command);

            var status = _fixture.GetApprenticeshipConfirmationStatus(command.ApprenticeshipId);

            status.Should().NotBeNull();
            status.CommitmentsApprovedOn.Should().Be(command.CommitmentsApprovedOn);
            status.ConfirmationOverdueOn.Should().Be(command.ConfirmationOverdueOn);
            status.ApprenticeshipConfirmedOn.Should().BeNull();
        }

        [Test]
        public async Task Handle_WhenAConfirmationStatusIsFoundForApprenticeship_ThenUpdateStatusIfThisChangeIsLater()
        {
            _fixture.SeedData();

            var command = _fixture.DataFixture.Build<ApprenticeshipConfirmationCommencedCommand>()
                .With(x=> x.CommitmentsApprovedOn, _fixture.ConfirmationStatus.CommitmentsApprovedOn.AddDays(1))
                .Create();

            await _fixture.Handle(command);

            var status = _fixture.GetApprenticeshipConfirmationStatus(command.ApprenticeshipId);

            status.Should().NotBeNull();
            status.CommitmentsApprovedOn.Should().Be(command.CommitmentsApprovedOn);
            status.ConfirmationOverdueOn.Should().Be(command.ConfirmationOverdueOn);
            status.ApprenticeshipConfirmedOn.Should().BeNull();
        }


        [Test]
        public async Task Handle_WhenAConfirmationStatusIsFoundForApprenticeship_ThenIgnoreIfThisChangeIsEarlier()
        {
            _fixture.SeedData();

            var command = _fixture.DataFixture.Build<ApprenticeshipConfirmationCommencedCommand>()
                .With(x => x.ApprenticeshipId, _fixture.ConfirmationStatus.ApprenticeshipId)
                .With(x => x.CommitmentsApprovedOn, _fixture.ConfirmationStatus.CommitmentsApprovedOn.AddDays(-1))
                .Create();

            await _fixture.Handle(command);

            var status = _fixture.GetApprenticeshipConfirmationStatus(command.ApprenticeshipId);

            status.Should().NotBeNull();
            status.CommitmentsApprovedOn.Should().Be(_fixture.ConfirmationStatus.CommitmentsApprovedOn);
            status.ApprenticeshipConfirmedOn.Should().Be(_fixture.ConfirmationStatus.ApprenticeshipConfirmedOn);
            status.ConfirmationOverdueOn.Should().Be(_fixture.ConfirmationStatus.ConfirmationOverdueOn);
            status.ConfirmationStatus.Should().Be(ConfirmationStatus.Confirmed);
        }

    }

    public class ApprenticeshipConfirmationCommencedCommandHandlerTestsFixture : IDisposable
    {
        public Fixture DataFixture { get; set; }
        public ApprenticeshipConfirmationStatus ConfirmationStatus { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public IRequestHandler<ApprenticeshipConfirmationCommencedCommand> Handler { get; set; }

        public ApprenticeshipConfirmationCommencedCommandHandlerTestsFixture()
        {
            DataFixture = new Fixture();

            ConfirmationStatus = DataFixture.Build<ApprenticeshipConfirmationStatus>().Without(x=>x.Apprenticeship).Create();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            Handler = new ApprenticeshipConfirmationCommencedCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle(ApprenticeshipConfirmationCommencedCommand command)
        {
            await Handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public ApprenticeshipConfirmationCommencedCommandHandlerTestsFixture SeedData()
        {
            Db.ApprenticeshipConfirmationStatus.Add(ConfirmationStatus);
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