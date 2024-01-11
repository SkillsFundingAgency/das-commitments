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
            var fixture = new CreateChangeOfPartyCommandHandlerTestsFixture();

            await fixture.Handle();

            fixture.ChangeOfPartyDomainService.Verify(s => s.CreateChangeOfPartyRequest(fixture.Command.ApprenticeshipId,
                fixture.Command.ChangeOfPartyRequestType, fixture.Command.NewPartyId, fixture.Command.NewPrice,
                fixture.Command.NewStartDate, fixture.Command.NewEndDate, fixture.Command.UserInfo, fixture.Command.NewEmploymentPrice,
                fixture.Command.NewEmploymentEndDate, fixture.Command.DeliveryModel, fixture.Command.HasOverlappingTrainingDates, It.IsAny<CancellationToken>()));
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