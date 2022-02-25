using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.BulkUploadControllerTests
{
    [TestFixture]
    public class ValidateBulkuploadTests
    {
        private ValidateBulkuploadTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ValidateBulkuploadTestsFixture();
        }

        [Test]
        public async Task BulkUploadValidate_VerifyCommandSend()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyCommandSend();
        }

        [Test]
        public async Task BulkUploadValidate_VerifyMapper()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyMapper();
        }

        private class ValidateBulkuploadTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IModelMapper> _mapper;
            private readonly BulkUploadController _controller;

            private readonly Fixture _autoFixture;
            private readonly long _apprenticeshipId;
            private readonly BulkUploadValidateApiRequest _postRequest;
            private readonly BulkUploadValidateCommand _command;

            public ValidateBulkuploadTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();

                _autoFixture = new Fixture();
                _apprenticeshipId = _autoFixture.Create<long>();
                _postRequest = _autoFixture.Create<BulkUploadValidateApiRequest>();
                _command = _autoFixture.Create<BulkUploadValidateCommand>();

                _mapper.Setup(x => x.Map<BulkUploadValidateCommand>(_postRequest)).ReturnsAsync(() => _command);
                _controller = new BulkUploadController(_mediator.Object, _mapper.Object, Mock.Of<ILogger<BulkUploadController>>());
            }

            public async Task BulkUploadDraftApprenticeshipsRequest()
            {
                await _controller.Validate(_postRequest);
            }

            public void VerifyMapper()
            {
                _mapper.Verify(m => m.Map<BulkUploadValidateCommand>(_postRequest), Times.Once);
            }

            public void VerifyCommandSend()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<BulkUploadValidateCommand>(
                            p => p.CsvRecords == _command.CsvRecords &&
                            p.ProviderId == _command.ProviderId),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
