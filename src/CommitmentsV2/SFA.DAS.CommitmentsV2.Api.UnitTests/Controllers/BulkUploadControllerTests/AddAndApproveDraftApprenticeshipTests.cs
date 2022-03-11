using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.BulkUploadControllerTests
{
    [TestFixture]
    public class AddAndApproveDraftApprenticeshipTests
    {
        private AddAndApproveDraftApprenticeshipTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddAndApproveDraftApprenticeshipTestsFixture();
        }

        [Test]
        public async Task BulkUploadDraftApprenticeshipsRequest_VerifyCommandSend()
        {
            //Act
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            
            //Assert
            _fixture.VerifyCommandSend();
        }

        [Test]
        public async Task BulkUploadDraftApprenticeshipsRequest_VerifyMapper()
        {
            //Act
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            
            //Assert
            _fixture.VerifyMapper();
        }

        private class AddAndApproveDraftApprenticeshipTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly BulkUploadController _controller;

            private readonly Fixture _autoFixture;            
            private readonly BulkUploadAddAndApproveDraftApprenticeshipsRequest _postRequest;
            private readonly BulkUploadAddAndApproveDraftApprenticeshipsCommand _command;

            public AddAndApproveDraftApprenticeshipTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _autoFixture = new Fixture();
                _autoFixture.Customizations.Add(new BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder());                
                _postRequest = _autoFixture.Create<BulkUploadAddAndApproveDraftApprenticeshipsRequest>();
                _command = _autoFixture.Create<BulkUploadAddAndApproveDraftApprenticeshipsCommand>();

                _mapper.Setup(x => x.Map<BulkUploadAddAndApproveDraftApprenticeshipsCommand>(_postRequest)).ReturnsAsync(() => _command);
                _controller = new BulkUploadController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<BulkUploadController>>());
            }

            public async Task BulkUploadDraftApprenticeshipsRequest()
            {
                await _controller.AddAndApproveDraftApprenticeships(_postRequest);
            }

            public void VerifyMapper()
            {
                _mapper.Verify(m => m.Map<BulkUploadAddAndApproveDraftApprenticeshipsCommand>(_postRequest), Times.Once);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<BulkUploadAddAndApproveDraftApprenticeshipsCommand>(
                            p => p.BulkUploadDraftApprenticeships == _command.BulkUploadDraftApprenticeships &&
                            p.UserInfo == _command.UserInfo),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    } 
}
