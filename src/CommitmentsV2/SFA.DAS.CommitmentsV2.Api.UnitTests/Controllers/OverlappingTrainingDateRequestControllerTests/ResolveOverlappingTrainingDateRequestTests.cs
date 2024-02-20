using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class ResolveOverlappingTrainingDateRequestTests
    {
        private ResolveOverlappingTrainingDateRequestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ResolveOverlappingTrainingDateRequestFixture();
        }

        [Test]
        public async Task ResolveOverlappingTrainingDateRequest_VerifyCommandSend()
        {
            await _fixture.ResolveOverlappingTrainingDateRequest();
            _fixture.VerifyCommandSend();
        }

        private class ResolveOverlappingTrainingDateRequestFixture
        {
            private readonly Mock<IMediator> _mediator;
            private Mock<IModelMapper> _mapper;
            private readonly OverlappingTrainingDateRequestController _controller;

            private readonly Fixture _autoFixture;
            private readonly ResolveApprenticeshipOverlappingTrainingDateRequest _postRequest;
            private readonly ResolveOverlappingTrainingDateRequestCommand _command;
            public const int ProviderId = 1;

            public ResolveOverlappingTrainingDateRequestFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();
                _postRequest = _autoFixture.Create<ResolveApprenticeshipOverlappingTrainingDateRequest>();

                _command = _autoFixture.Create<ResolveOverlappingTrainingDateRequestCommand>();

                _mediator.Setup(x => x.Send(_command, It.IsAny<CancellationToken>()));
                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task ResolveOverlappingTrainingDateRequest()
            {
                await _controller.Resolve(_postRequest);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<ResolveOverlappingTrainingDateRequestCommand>(
                            p => p.ApprenticeshipId == _postRequest.ApprenticeshipId &&
                            p.ResolutionType == _postRequest.ResolutionType),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}