using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CreateChangeOfPartyCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldCallDomainServiceCorrectly()
        {
            var f = new CreateChangeOfPartyCommandHandlerTestsFixture();
            
            await f.Handle();

            f.ChangeOfPartyDomainService.Verify(s => s.CreateChangeOfPartyRequest(f.Command.ApprenticeshipId,
                f.Command.ChangeOfPartyRequestType, f.Command.NewPartyId, f.Command.NewPrice,
                f.Command.NewStartDate, f.Command.NewEndDate, f.Command.UserInfo, f.Command.NewEmploymentPrice,
                f.Command.NewEmploymentEndDate,  f.Command.DeliveryModel, It.IsAny<CancellationToken>()));
        }
    }

    public class CreateChangeOfPartyCommandHandlerTestsFixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<IChangeOfPartyRequestDomainService> ChangeOfPartyDomainService { get; set; }
        public IRequestHandler<CreateChangeOfPartyRequestCommand> Handler { get; set; }
        public CreateChangeOfPartyRequestCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public CreateChangeOfPartyCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            ChangeOfPartyDomainService = new Mock<IChangeOfPartyRequestDomainService>();
            Handler = new CreateChangeOfPartyRequestCommandHandler(ChangeOfPartyDomainService.Object);
            Command = AutoFixture.Create<CreateChangeOfPartyRequestCommand>();
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}