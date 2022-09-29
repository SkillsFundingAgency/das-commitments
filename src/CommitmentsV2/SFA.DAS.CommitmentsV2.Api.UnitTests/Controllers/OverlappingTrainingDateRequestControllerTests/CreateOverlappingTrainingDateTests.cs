using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class CreateOverlappingTrainingDateTests
    {
        private CreateOverlappingTrainingDateFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CreateOverlappingTrainingDateFixture();
        }

        [Test]
        public async Task BulkUploadValidate_VerifyCommandSend()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyCommandSend();
        }

        private class CreateOverlappingTrainingDateFixture
        {
            private readonly Mock<IMediator> _mediator;
            private Mock<IModelMapper> _mapper;
            private readonly OverlappingTrainingDateRequestController _controller;

            private readonly Fixture _autoFixture;
            private readonly CreateOverlappingTrainingDateRequest _postRequest;
            private readonly CreateOverlappingTrainingDateResult _result;
            public const int ProviderId = 1;

            public CreateOverlappingTrainingDateFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();
                _postRequest = _autoFixture.Create<CreateOverlappingTrainingDateRequest>();
                _result = _autoFixture.Create<CreateOverlappingTrainingDateResult>();

                _mediator.Setup(x => x.Send(It.IsAny<CreateOverlappingTrainingDateRequestCommand>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(() => _result);

                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task BulkUploadDraftApprenticeshipsRequest()
            {
                await _controller.CreateOverlappingTrainingDate(ProviderId, _postRequest);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<CreateOverlappingTrainingDateRequestCommand>(
                            p => p.UserInfo == _postRequest.UserInfo
                            && p.DraftApprneticeshipId == _postRequest.DraftApprenticeshipId
                            && p.ProviderId == ProviderId),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}