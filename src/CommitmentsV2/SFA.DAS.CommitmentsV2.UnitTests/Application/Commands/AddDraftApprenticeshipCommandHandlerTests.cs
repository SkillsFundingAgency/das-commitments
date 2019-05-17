using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.Testing;

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
                f => f.CohortDomainServiceMock.Verify(c => c.AddDraftApprenticeship(f.Command.ProviderId, f.Command.CohortId, f.DraftApprenticeshipDetails, f.CancellationToken)));
        }
    }

    public class AddDraftApprenticeshipCommandHandlerTestsFixture
    {
        public Fixture Fixture { get; set; }
        public AddDraftApprenticeshipCommand Command { get; set; }
        public DraftApprenticeshipDetails DraftApprenticeshipDetails { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Mock<ICohortDomainService> CohortDomainServiceMock { get; set; }
        public Mock<IAsyncMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapperMock { get; set; }
        public IRequestHandler<AddDraftApprenticeshipCommand> Handler { get; set; }

        public AddDraftApprenticeshipCommandHandlerTestsFixture()
        {
            Fixture = new Fixture();
            Command = Fixture.Create<AddDraftApprenticeshipCommand>();
            DraftApprenticeshipDetails = new DraftApprenticeshipDetails();
            CancellationToken = new CancellationToken();
            CohortDomainServiceMock = new Mock<ICohortDomainService>();
            DraftApprenticeshipDetailsMapperMock = new Mock<IAsyncMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>>();
            Handler = new AddDraftApprenticeshipCommandHandler(DraftApprenticeshipDetailsMapperMock.Object, CohortDomainServiceMock.Object);

            DraftApprenticeshipDetailsMapperMock.Setup(m => m.Map(Command)).ReturnsAsync(DraftApprenticeshipDetails);
        }

        public Task AddDraftApprenticeship()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}