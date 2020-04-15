using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ChangeOfPartyControllerTests
{
    public class ChangeOfPartyControllerTests
    {
        private ChangeOfPartyControllerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyControllerTestsFixture();
        }

        [Test]
        public async Task PostChangeOfPartyRequest()
        {
            await _fixture.PostChangeOfPartyRequest();
            _fixture.VerifyPost();
        }

        private class ChangeOfPartyControllerTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly ChangeOfPartyController _controller;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;
            private readonly CreateChangeOfPartyRequestRequest _postRequest;

            public ChangeOfPartyControllerTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _autoFixture = new Fixture();
                _apprenticeshipId = _autoFixture.Create<long>();
                _postRequest = _autoFixture.Create<CreateChangeOfPartyRequestRequest>();

                _controller = new ChangeOfPartyController(_mediator.Object, _mapper.Object);
            }

            public async Task PostChangeOfPartyRequest()
            {
                await _controller.CreateChangeOfPartyRequest(_apprenticeshipId, _postRequest);
            }

            public void VerifyPost()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<ChangeOfPartyRequestCommand>(p =>
                            p.ApprenticeshipId == _apprenticeshipId &&
                            p.ChangeOfPartyRequestType == _postRequest.ChangeOfPartyRequestType &&
                            p.NewPartyId == _postRequest.NewPartyId && p.NewStartDate == _postRequest.NewStartDate &&
                            p.NewPrice == _postRequest.NewPrice && p.UserInfo == _postRequest.UserInfo),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
