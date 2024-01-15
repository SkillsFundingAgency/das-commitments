using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CreateOverlappingTrainingDateRequestCommandHandlerTests
    {
        [Test]
        public async Task Handle_WhenHandlingCommand_ThenShouldCallDomainServiceCorrectly()
        {
            var fixture = new CreateOverlappingTrainingDateRequestCommandHandlerTestsFixture();

            await fixture.Handle();

            fixture.OverlappingTrainingDateRequestService.Verify(s => s.CreateOverlappingTrainingDateRequest(fixture.Command.DraftApprenticeshipId,
                fixture.OriginatingParty, null, fixture.Command.UserInfo, It.IsAny<CancellationToken>()));
        }
    }

    public class CreateOverlappingTrainingDateRequestCommandHandlerTestsFixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<IOverlappingTrainingDateRequestDomainService> OverlappingTrainingDateRequestService { get; set; }
        public Mock<IAuthenticationService> AuthenticationService { get; set; }
        public IRequestHandler<CreateOverlappingTrainingDateRequestCommand, CreateOverlappingTrainingDateResult> Handler { get; set; }
        public CreateOverlappingTrainingDateRequestCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public OverlappingTrainingDateRequest Response { get; set; }
        public Party OriginatingParty { get; set; }

        public CreateOverlappingTrainingDateRequestCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            OverlappingTrainingDateRequestService = new Mock<IOverlappingTrainingDateRequestDomainService>();
            AuthenticationService = new Mock<IAuthenticationService>();
            Response = AutoFixture.Create<OverlappingTrainingDateRequest>();

            OriginatingParty = Party.Provider;
            AuthenticationService.Setup(x => x.GetUserParty()).Returns(OriginatingParty);

            OverlappingTrainingDateRequestService.Setup(x => x.CreateOverlappingTrainingDateRequest(It.IsAny<long>(), OriginatingParty, null, It.IsAny<UserInfo>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response);

            Handler = new CreateOverlappingTrainingDateRequestCommandHandler(OverlappingTrainingDateRequestService.Object, AuthenticationService.Object);
            Command = AutoFixture.Create<CreateOverlappingTrainingDateRequestCommand>();
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }
    }
}