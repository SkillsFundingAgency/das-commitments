using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.SendCohort;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class SendCohortCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldSendCohortToOtherParty()
        {
            var f = new SendCohortCommandHandlerTestsFixture();
            await f.Handle();

            f.CohortDomainService.Verify(s => s.SendCohortToOtherParty(f.Command.CohortId, f.Command.Message,
                f.Command.UserInfo, f.Command.RequestingParty, f.CancellationToken));
        }
    }

    public class SendCohortCommandHandlerTestsFixture
    {
        public IFixture AutoFixture { get; set; }
        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<SendCohortCommand> Handler { get; set; }
        public SendCohortCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public SendCohortCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            CohortDomainService = new Mock<ICohortDomainService>();
            Handler = new SendCohortCommandHandler(CohortDomainService.Object);
            Command = AutoFixture.Create<SendCohortCommand>();
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}