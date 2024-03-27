using SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ApproveCohortCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldApproveCohort()
        {
            var fixture = new ApproveCohortCommandHandlerTestsFixture();
            
            await fixture.Handle();

            fixture.CohortDomainService.Verify(s => s.ApproveCohort(fixture.Command.CohortId, fixture.Command.Message, fixture.Command.UserInfo, fixture.Command.RequestingParty, fixture.CancellationToken));
        }
    }

    public class ApproveCohortCommandHandlerTestsFixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<ApproveCohortCommand> Handler { get; set; }
        public ApproveCohortCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public ApproveCohortCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            CohortDomainService = new Mock<ICohortDomainService>();
            Handler = new ApproveCohortCommandHandler(CohortDomainService.Object);
            Command = AutoFixture.Create<ApproveCohortCommand>();
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}