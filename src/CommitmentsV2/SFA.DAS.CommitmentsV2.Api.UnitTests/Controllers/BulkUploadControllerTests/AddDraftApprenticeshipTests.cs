using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.BulkUploadControllerTests
{
    [TestFixture]
    public class AddDraftApprenticeshipTests
    {
        private AddDraftApprenticeshipTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddDraftApprenticeshipTestsFixture();
        }

        [Test]
        public async Task BulkUploadDraftApprenticeshipsRequest_VerifyCommandSend()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyCommandSend();
        }

        [Test]
        public async Task BulkUploadDraftApprenticeshipsRequest_VerifyMapper()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyMapper();
        }

        private class AddDraftApprenticeshipTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly BulkUploadController _controller;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;
            private readonly BulkUploadAddDraftApprenticeshipsRequest _postRequest;
            private readonly BulkUploadAddDraftApprenticeshipsCommand _command;

            public AddDraftApprenticeshipTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _autoFixture = new Fixture();
                _apprenticeshipId = _autoFixture.Create<long>();
                _postRequest = _autoFixture.Create<BulkUploadAddDraftApprenticeshipsRequest>();
                _command = _autoFixture.Create<BulkUploadAddDraftApprenticeshipsCommand>();

                _mapper.Setup(x => x.Map<BulkUploadAddDraftApprenticeshipsCommand>(_postRequest)).ReturnsAsync(() => _command);
                _controller = new BulkUploadController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<BulkUploadController>>());
            }

            public async Task BulkUploadDraftApprenticeshipsRequest()
            {
                await _controller.AddDraftApprenticeships(_postRequest);
            }

            public void VerifyMapper()
            {
                _mapper.Verify(m => m.Map<BulkUploadAddDraftApprenticeshipsCommand>(_postRequest), Times.Once);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<BulkUploadAddDraftApprenticeshipsCommand>(
                            p => p.BulkUploadDraftApprenticeships == _command.BulkUploadDraftApprenticeships &&
                            p.UserInfo == _command.UserInfo),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
