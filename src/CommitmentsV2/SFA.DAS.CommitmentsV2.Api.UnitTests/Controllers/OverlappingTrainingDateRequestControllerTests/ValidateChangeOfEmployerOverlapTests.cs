using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateChangeOfEmployerOverlap;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class ValidateChangeOfEmployerOverlapTests
    {
        private ValidateChangeOfEmployerOverlapFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ValidateChangeOfEmployerOverlapFixture();
        }

        [Test]
        public async Task ValidateChangeOfEmployerOverlap_VerifyCommandSend()
        {
            await _fixture.ValidateChangeOfEmployerOverlap();
            _fixture.VerifyCommandSend();
        }

        private class ValidateChangeOfEmployerOverlapFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly OverlappingTrainingDateRequestController _controller;

            private readonly Fixture _autoFixture;
            private readonly ValidateChangeOfEmployerOverlapRequest _postRequest;
            private readonly ValidateChangeOfEmployerOverlapCommand _command;
            public const int ProviderId = 1;

            public ValidateChangeOfEmployerOverlapFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();
                _postRequest = _autoFixture.Create<ValidateChangeOfEmployerOverlapRequest>();

                _command = _autoFixture.Create<ValidateChangeOfEmployerOverlapCommand>();

                _mediator.Setup(x => x.Send(_command, It.IsAny<CancellationToken>()));
                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task ValidateChangeOfEmployerOverlap()
            {
                await _controller.ValidateChangeOfEmployerOverlap(ProviderId, _postRequest);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<ValidateChangeOfEmployerOverlapCommand>(
                            p => p.ProviderId == ProviderId &&
                            p.Uln == _postRequest.Uln &&
                            p.EndDate == _postRequest.EndDate &&
                            p.StartDate == _postRequest.StartDate
                            ),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}