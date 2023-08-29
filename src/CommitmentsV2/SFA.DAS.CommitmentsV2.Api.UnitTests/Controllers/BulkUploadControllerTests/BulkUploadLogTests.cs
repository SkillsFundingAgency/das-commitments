using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task BulkUploadValidate_VerifyCommandSend()
        {
            await _fixture.BulkUploadAddLogRequest();
            _fixture.VerifyCommandSend();
        }

        [Test]
        public async Task BulkUploadValidate_VerifyMapper()
        {
            await _fixture.BulkUploadAddLogRequest();
            _fixture.VerifyMapper();
        }

        private class BulkUploadLogTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly BulkUploadController _controller;

            private readonly Fixture _autoFixture;
            private readonly long _logId;
            private readonly AddFileUploadLogRequest _postRequest;
            private readonly AddFileUploadLogCommand _command;

            public BulkUploadLogTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();
                _logId = _autoFixture.Create<long>();
                _postRequest = _autoFixture.Create<AddFileUploadLogRequest>();

                _command = _autoFixture.Create<AddFileUploadLogCommand>();

                _mapper.Setup(x => x.Map<AddFileUploadLogCommand>(_postRequest)).ReturnsAsync(() => _command);
                _mediator.Setup(x => x.Send(_command, It.IsAny<CancellationToken>())).ReturnsAsync(() => new BulkUploadAddLogResponse { LogId = _logId });
                _controller = new BulkUploadController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<BulkUploadController>>());
            }

            public async Task BulkUploadAddLogRequest()
            {
               await _controller.AddLog(_postRequest);
            }

            public void VerifyMapper()
            {
                _mapper.Verify(m => m.Map<AddFileUploadLogCommand>(_postRequest), Times.Once);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<AddFileUploadLogCommand>(
                            p =>
                            p.ProviderId == _command.ProviderId &&
                            p.FileName == _command.FileName &&
                            p.RowCount == _command.RowCount &&
                            p.RplCount == _command.RplCount &&
                            p.FileContent == _command.FileContent
                            ),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
