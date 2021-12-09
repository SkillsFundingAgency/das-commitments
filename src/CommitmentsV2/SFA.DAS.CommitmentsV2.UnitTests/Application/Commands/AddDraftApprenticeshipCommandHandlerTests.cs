using System;
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
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddDraftApprenticeshipCommandHandlerTests : FluentTest<AddDraftApprenticeshipCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenCommandIsHandled_ThenShouldAddDraftApprenticeship()
        {
            return TestAsync(
                f => f.AddDraftApprenticeship(),
                f => f.CohortDomainService.Verify(c => c.AddDraftApprenticeship(f.Command.ProviderId,
                    f.Command.CohortId, f.DraftApprenticeshipDetails, f.UserInfo, f.CancellationToken)));
        }

        [Test]
        public Task Handle_WhenCommandIsHandled_ThenShouldReturnAddDraftApprenticeshipResult()
        {
            return TestAsync(
                f => f.AddDraftApprenticeship(),
                (f, r) => r.Should().NotBeNull().And.Subject.Should().Match<AddDraftApprenticeshipResult>(r2 => r2.Id == f.DraftApprenticeship.Id));
        }
    }

    public class AddDraftApprenticeshipCommandHandlerTestsFixture
    {
        public Fixture Fixture { get; set; }
        public AddDraftApprenticeshipCommand Command { get; set; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
        public DraftApprenticeship DraftApprenticeship { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public Mock<IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapper { get; set; }

        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<AddDraftApprenticeshipCommand, AddDraftApprenticeshipResult> Handler { get; set; }
        public UserInfo UserInfo { get; }

        public AddDraftApprenticeshipCommandHandlerTestsFixture()
        {
            Fixture = new Fixture();
            DraftApprenticeshipDetails = Fixture.Create<DraftApprenticeshipDetails>();
            DraftApprenticeship = new DraftApprenticeship().Set(a => a.Id, 123);
            CancellationToken = new CancellationToken();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            CohortDomainService = new Mock<ICohortDomainService>();
            DraftApprenticeshipDetailsMapper = new Mock<IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>>();
            UserInfo = Fixture.Create<UserInfo>();
            Command = Fixture.Build<AddDraftApprenticeshipCommand>().With(o => o.UserInfo, UserInfo).Create();

            Handler = new AddDraftApprenticeshipCommandHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db),
                Mock.Of<ILogger<AddDraftApprenticeshipCommandHandler>>(),
                DraftApprenticeshipDetailsMapper.Object,
                CohortDomainService.Object);

            CohortDomainService.Setup(s => s.AddDraftApprenticeship(Command.ProviderId, Command.CohortId,
                DraftApprenticeshipDetails, Command.UserInfo, CancellationToken)).ReturnsAsync(DraftApprenticeship);
            DraftApprenticeshipDetailsMapper.Setup(m => m.Map(Command)).ReturnsAsync(DraftApprenticeshipDetails);
        }

        public Task<AddDraftApprenticeshipResult> AddDraftApprenticeship()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}