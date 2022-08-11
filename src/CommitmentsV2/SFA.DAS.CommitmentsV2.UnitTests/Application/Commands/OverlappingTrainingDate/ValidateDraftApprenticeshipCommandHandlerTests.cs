using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.OverlappingTrainingDate
{
    public class ValidateDraftApprenticeshipCommandHandlerTests
    {
        private ValidateDraftApprenticeshipFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ValidateDraftApprenticeshipFixture();
        }

        [Test]
        public async Task BulkUploadValidate_VerifyDraftApprenticeshipValidated()
        {
            await _fixture.BulkUploadDraftApprenticeshipsRequest();
            _fixture.VerifyCommandSend();
        }

        private class ValidateDraftApprenticeshipFixture
        {
            private readonly ValidateDraftApprenticeshipCommandHandler _handler;
            private readonly Mock<IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails>> _mapper;
            private readonly Fixture _autoFixture;
            private readonly ValidateDraftApprenticeshipDetailsCommand _command;
            private readonly DraftApprenticeshipDetails _draftApprenticeshipDetails;
            private readonly Mock<ICohortDomainService> _cohortDomainService;
            public const int ProviderId = 1;

            public ValidateDraftApprenticeshipFixture()
            {
                _autoFixture = new Fixture();
                _command = _autoFixture.Create<ValidateDraftApprenticeshipDetailsCommand>();
                _draftApprenticeshipDetails = _autoFixture.Create<DraftApprenticeshipDetails>();
                _mapper = new Mock<IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails>>();
                _cohortDomainService = new Mock<ICohortDomainService>();

                _mapper.Setup(x => x.Map(_command)).ReturnsAsync(() => _draftApprenticeshipDetails);

                _handler = new ValidateDraftApprenticeshipCommandHandler(Mock.Of<ILogger<ValidateDraftApprenticeshipCommandHandler>>(), _mapper.Object, _cohortDomainService.Object);
            }

            public async Task BulkUploadDraftApprenticeshipsRequest()
            {
                await _handler.Handle(_command, CancellationToken.None);
            }

            public void VerifyCommandSend()
            {
                _cohortDomainService.Verify(
                    m => m.ValidateDraftApprenticeshipForOverlappingTrainingDateRequest(_command.DraftApprenticeshipRequest.ProviderId,
                    _command.DraftApprenticeshipRequest.CohortId,
                    _draftApprenticeshipDetails,
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
