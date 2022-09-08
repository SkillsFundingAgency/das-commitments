using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeship;
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
    public class ValidateDraftApprenticeshipCommandHandlerTests : FluentTest<ValidateDraftApprenticeshipCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenCommandIsHandled_ThenShouldAddDraftApprenticeship()
        {
            return TestAsync(
                f => f.AddDraftApprenticeship(),
                f => f.CohortDomainService.Verify(c => c.AddDraftApprenticeship(f.Command.ProviderId,
                    f.Command.CohortId, f.DraftApprenticeshipDetails, f.UserInfo, f.CancellationToken)));
        }
    }

    public class ValidateDraftApprenticeshipCommandHandlerTestsFixture
    {
        public Fixture Fixture { get; set; }
        public ValidateDraftApprenticeshipCommand Command { get; set; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
        public DraftApprenticeship DraftApprenticeship { get; set; }
        public CancellationToken CancellationToken { get; set; }
        
        public Mock<IOldMapper<DraftApprenticeshipCommandBase, DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapper { get; set; }

        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<ValidateDraftApprenticeshipCommand, ValidateDraftApprenticeshipResult> Handler { get; set; }
        public UserInfo UserInfo { get; }

        public ValidateDraftApprenticeshipCommandHandlerTestsFixture()
        {
            Fixture = new Fixture();
            DraftApprenticeshipDetails = Fixture.Build<DraftApprenticeshipDetails>()
                .With(o => o.IgnoreStartDateOverlap,false)
                .Create();
            DraftApprenticeship = new DraftApprenticeship().Set(a => a.Id, 123);
            CancellationToken = new CancellationToken();

            CohortDomainService = new Mock<ICohortDomainService>();
            DraftApprenticeshipDetailsMapper = new Mock<IOldMapper<DraftApprenticeshipCommandBase, DraftApprenticeshipDetails>>();
            UserInfo = Fixture.Create<UserInfo>();

            Command = Fixture.Build<ValidateDraftApprenticeshipCommand>().With(o => o.UserInfo, UserInfo).Without(x => x.IgnoreStartDateOverlap).Create();

            Handler = new ValidateDraftApprenticeshipCommandHandler(
                DraftApprenticeshipDetailsMapper.Object,
                CohortDomainService.Object);

            CohortDomainService.Setup(s => s.AddDraftApprenticeship(Command.ProviderId, Command.CohortId,
                DraftApprenticeshipDetails, Command.UserInfo, CancellationToken)).ReturnsAsync(DraftApprenticeship);
            DraftApprenticeshipDetailsMapper.Setup(m => m.Map(Command)).ReturnsAsync(DraftApprenticeshipDetails);
        }

        public Task<ValidateDraftApprenticeshipResult> AddDraftApprenticeship()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}