using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class ValidateDraftApprenticeshipDetailsTests
    {
        private ValidateDraftApprenticeshipFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ValidateDraftApprenticeshipFixture();
        }

        [Test]
        public async Task BulkUploadValidate_VerifyCommandSend()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyCommandSend();
        }

        private class ValidateDraftApprenticeshipFixture
        {
            private readonly Mock<IMediator> _mediator;
            private Mock<IModelMapper> _mapper;
            private readonly OverlappingTrainingDateRequestController _controller;

            private readonly Fixture _autoFixture;
            private readonly ValidateDraftApprenticeshipRequest _postRequest;
            private readonly ValidateDraftApprenticeshipDetailsCommand _command;
            public const int ProviderId = 1;

            public ValidateDraftApprenticeshipFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();
                _postRequest = _autoFixture.Create<ValidateDraftApprenticeshipRequest>();

                _command = _autoFixture.Create<ValidateDraftApprenticeshipDetailsCommand>();

                _mediator.Setup(x => x.Send(_command, It.IsAny<CancellationToken>()));
                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task BulkUploadDraftApprenticeshipsRequest()
            {
                await _controller.ValidateDraftApprenticeship(ProviderId, _postRequest);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<ValidateDraftApprenticeshipDetailsCommand>(
                            p => p.DraftApprenticeshipRequest == _postRequest),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}