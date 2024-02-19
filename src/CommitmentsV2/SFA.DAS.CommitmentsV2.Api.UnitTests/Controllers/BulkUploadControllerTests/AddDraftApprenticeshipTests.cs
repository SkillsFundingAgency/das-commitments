using AutoFixture.Kernel;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

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
                _autoFixture.Customizations.Add(new BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder());
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

    public class BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(BulkUploadAddDraftApprenticeshipRequest))
            {
                var fixture = new Fixture();
                var startDate = fixture.Create<DateTime>();
                var endDate = fixture.Create<DateTime>();
                var dob = fixture.Create<DateTime>();
                return fixture.Build<BulkUploadAddDraftApprenticeshipRequest>()
                    .With(x => x.StartDateAsString, startDate.ToString("yyyy-MM-dd"))
                    .With(x => x.EndDateAsString, endDate.ToString("yyyy-MM"))
                    .With(x => x.DateOfBirthAsString, dob.ToString("yyyy-MM-dd"))
                    .With(x => x.CostAsString, "1000")
                    .Create();
            }

            return new NoSpecimen();
        }
    }
}
