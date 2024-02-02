using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldAddDraftApprenticeship()
        {
            using var fixture = new AddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.AddDraftApprenticeship();

            fixture.CohortDomainService.Verify(c => c.AddDraftApprenticeship(fixture.Command.ProviderId,
                fixture.Command.CohortId, fixture.DraftApprenticeshipDetails, fixture.UserInfo,
                fixture.Command.RequestingParty, fixture.CancellationToken));
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldReturnAddDraftApprenticeshipResult()
        {
            using var fixture = new AddDraftApprenticeshipCommandHandlerTestsFixture();

            var result = await fixture.AddDraftApprenticeship();

            result.Should().NotBeNull()
                .And.Subject.Should()
                .Match<AddDraftApprenticeshipResult>(r2 => r2.Id == fixture.DraftApprenticeship.Id);
        }
    }

    public class AddDraftApprenticeshipCommandHandlerTestsFixture : IDisposable
    {
        public Fixture Fixture { get; set; }
        public AddDraftApprenticeshipCommand Command { get; set; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
        public DraftApprenticeship DraftApprenticeship { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public Mock<IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>>
            DraftApprenticeshipDetailsMapper { get; set; }

        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<AddDraftApprenticeshipCommand, AddDraftApprenticeshipResult> Handler { get; set; }
        public UserInfo UserInfo { get; }

        public AddDraftApprenticeshipCommandHandlerTestsFixture()
        {
            Fixture = new Fixture();
            DraftApprenticeshipDetails = Fixture.Build<DraftApprenticeshipDetails>()
                .With(o => o.IgnoreStartDateOverlap, false)
                .Create();
            DraftApprenticeship = new DraftApprenticeship().Set(a => a.Id, 123);
            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            CohortDomainService = new Mock<ICohortDomainService>();
            DraftApprenticeshipDetailsMapper =
                new Mock<IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>>();
            UserInfo = Fixture.Create<UserInfo>();

            Command = Fixture.Build<AddDraftApprenticeshipCommand>().With(o => o.UserInfo, UserInfo)
                .Without(x => x.IgnoreStartDateOverlap).Create();

            Handler = new AddDraftApprenticeshipCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                Mock.Of<ILogger<AddDraftApprenticeshipCommandHandler>>(),
                DraftApprenticeshipDetailsMapper.Object,
                CohortDomainService.Object);

            CohortDomainService.Setup(s => s.AddDraftApprenticeship(Command.ProviderId, Command.CohortId,
                    DraftApprenticeshipDetails, Command.UserInfo, Command.RequestingParty, CancellationToken))
                .ReturnsAsync(DraftApprenticeship);
            DraftApprenticeshipDetailsMapper.Setup(m => m.Map(Command)).ReturnsAsync(DraftApprenticeshipDetails);
        }

        public Task<AddDraftApprenticeshipResult> AddDraftApprenticeship()
        {
            return Handler.Handle(Command, CancellationToken);
        }

        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}