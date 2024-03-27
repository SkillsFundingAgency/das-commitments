using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Application.Commands.FileUploadLogUpdateWithErrorContent;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.BulkUploadControllerTests
{
    [TestFixture]
    public class BulkUploadLogTests
    {
        private BulkUploadLogTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new BulkUploadLogTestsFixture();
        }

        [Test]
        public async Task BulkUploadLog_VerifyCommandSend()
        {
            await _fixture.BulkUploadAddLogRequest();
            _fixture.VerifyAddLogCommandSent();
        }

        [Test]
        public async Task BulkUploadLog_VerifyMapper()
        {
            await _fixture.BulkUploadAddLogRequest();
            _fixture.VerifyMapper();
        }


        [Test]
        public async Task BulkUploadValidate_VerifyCommandSend()
        {
            await _fixture.BulkUploadLogUpdatedWithErrorContent();
            _fixture.VerifyUpdateLogCommandSent();
        }

        private class BulkUploadLogTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly BulkUploadController _controller;

            private readonly Fixture _autoFixture;
            private readonly long _logId;
            private readonly long _providerId;
            private readonly AddFileUploadLogRequest _postRequest;
            private readonly FileUploadLogUpdateWithErrorContentRequest _putRequest;
            private readonly AddFileUploadLogCommand _addCommand;
            //private readonly FileUploadLogUpdateWithErrorContentRequest _updateRequest;

            public BulkUploadLogTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();
                _logId = _autoFixture.Create<long>();
                _providerId = _autoFixture.Create<long>();
                _postRequest = _autoFixture.Create<AddFileUploadLogRequest>();
                _putRequest = _autoFixture.Create<FileUploadLogUpdateWithErrorContentRequest>();
                _addCommand = _autoFixture.Create<AddFileUploadLogCommand>();

                _mapper.Setup(x => x.Map<AddFileUploadLogCommand>(_postRequest)).ReturnsAsync(() => _addCommand);
                _mediator.Setup(x => x.Send(_addCommand, It.IsAny<CancellationToken>())).ReturnsAsync(() => new BulkUploadAddLogResponse { LogId = _logId });
                _controller = new BulkUploadController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<BulkUploadController>>());
            }

            public async Task BulkUploadAddLogRequest()
            {
               await _controller.AddLog(_postRequest);
            }

            public async Task BulkUploadLogUpdatedWithErrorContent()
            {
                await _controller.UpdateLogErrorContent(_providerId, _logId, _putRequest);
            }

            public void VerifyMapper()
            {
                _mapper.Verify(m => m.Map<AddFileUploadLogCommand>(_postRequest), Times.Once);
            }

            public void VerifyAddLogCommandSent()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<AddFileUploadLogCommand>(
                            p =>
                            p.ProviderId == _addCommand.ProviderId &&
                            p.FileName == _addCommand.FileName &&
                            p.RowCount == _addCommand.RowCount &&
                            p.RplCount == _addCommand.RplCount &&
                            p.FileContent == _addCommand.FileContent
                            ),
                        It.IsAny<CancellationToken>()), Times.Once);
            }

            public void  VerifyUpdateLogCommandSent()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<FileUploadLogUpdateWithErrorContentCommand>(
                            p =>
                                p.ProviderId == _providerId &&
                                p.LogId == _logId &&
                                p.ErrorContent == _putRequest.ErrorContent
                        ),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
