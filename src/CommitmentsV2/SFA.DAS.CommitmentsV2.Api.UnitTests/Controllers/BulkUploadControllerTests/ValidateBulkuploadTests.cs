using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

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
        public void BulkUploadValidate_VerifyExceptionThrown_When_Errors()
        {
            _fixture.WithErrors();
            Assert.ThrowsAsync<BulkUploadDomainException>(() => _fixture.BulkUploadDraftApprenticeshipsRequest());
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
                _mediator.Setup(x => x.Send(_command, It.IsAny<CancellationToken>())).ReturnsAsync(() => new BulkUploadValidateApiResponse {  BulkUploadValidationErrors = new List<BulkUploadValidationError>()});
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

            public void WithErrors()
            {
                var errorResponse = _autoFixture.Create<BulkUploadValidateApiResponse>();
                _mediator.Setup(x => x.Send(_command, It.IsAny<CancellationToken>())).ReturnsAsync(() => errorResponse);
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
