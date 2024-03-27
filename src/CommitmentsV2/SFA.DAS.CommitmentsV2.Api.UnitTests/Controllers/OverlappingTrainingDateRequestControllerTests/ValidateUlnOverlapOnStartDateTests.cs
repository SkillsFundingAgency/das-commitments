using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class GetOverlapOnStartDateTests
    {
        private ValidateUlnOverlapOnStartDateFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ValidateUlnOverlapOnStartDateFixture();
        }

        [Test]
        public async Task ValidateUlnOverlapOnStartDate_VerifyQuerySent()
        {
            await _fixture.ValidateUlnOverlapOnStartDate();
            _fixture.VerifyQuerySent();
        }

        private class ValidateUlnOverlapOnStartDateFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly OverlappingTrainingDateRequestController _controller;
            private Mock<IModelMapper> _mapper;
            private readonly Fixture _autoFixture;
            public const int ProviderId = 1;
            public string Uln;
            public string StartDate;
            public string EndDate;

            public ValidateUlnOverlapOnStartDateFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();

                Uln = "9235610906";
                StartDate = "Jan 2022";
                EndDate = "Dec 2022";

                var queryResult = _autoFixture.Create<ValidateUlnOverlapOnStartDateQueryResult>();
                _mediator
                    .Setup(x => x.Send(It.IsAny<ValidateUlnOverlapOnStartDateQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(queryResult);

                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task ValidateUlnOverlapOnStartDate()
            {
                await _controller.ValidateUlnOverlapOnStartDate(ProviderId, Uln, StartDate, EndDate);
            }

            public void VerifyQuerySent()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<ValidateUlnOverlapOnStartDateQuery>(p => p.ProviderId == ProviderId
                        && p.Uln == Uln),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}