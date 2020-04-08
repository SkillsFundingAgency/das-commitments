using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class ChangeOfPartyCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldCallDomainServiceCorrectly()
        {
            var f = new ChangeOfPartyCommandHandlerTestsFixture();
            
            await f.Handle();

            f.ChangeOfPartyDomainService.Verify(s => s.CreateChangeOfPartyRequest(f.Command.ApprenticeshipId,
                f.Command.ChangeOfPartyRequestType, f.Command.NewPartyId, f.Command.NewPrice.Value,
                f.Command.NewStartDate.Value, null, f.Command.UserInfo, It.IsAny<CancellationToken>()));
        }
    }

    public class ChangeOfPartyCommandHandlerTestsFixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<IChangeOfPartyRequestDomainService> ChangeOfPartyDomainService { get; set; }
        public IRequestHandler<ChangeOfPartyRequestCommand> Handler { get; set; }
        public ChangeOfPartyRequestCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public ChangeOfPartyCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            ChangeOfPartyDomainService = new Mock<IChangeOfPartyRequestDomainService>();
            Handler = new ChangeOfPartyRequestCommandHandler(ChangeOfPartyDomainService.Object);
            Command = AutoFixture.Create<ChangeOfPartyRequestCommand>();
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}