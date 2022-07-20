using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ChangeOfPartyControllerTests
{
    [TestFixture]
    public class CreateTests
    {
        private CreateTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CreateTestsFixture();
        }

        [Test]
        public async Task PostChangeOfPartyRequest()
        {
            await _fixture.PostChangeOfPartyRequest();
            _fixture.VerifyPost();
        }

        private class CreateTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly ChangeOfPartyController _controller;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;
            private readonly CreateChangeOfPartyRequestRequest _postRequest;

            public CreateTestsFixture()
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
                        It.Is<CreateChangeOfPartyRequestCommand>(p =>
                            p.ApprenticeshipId == _apprenticeshipId &&
                            p.ChangeOfPartyRequestType == _postRequest.ChangeOfPartyRequestType &&
                            p.NewPartyId == _postRequest.NewPartyId &&
                            p.NewStartDate == _postRequest.NewStartDate &&
                            p.NewPrice == _postRequest.NewPrice &&
                            p.UserInfo == _postRequest.UserInfo &&
                            p.NewEndDate == _postRequest.NewEndDate &&
                            p.DeliveryModel == _postRequest.DeliveryModel
                            ),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
