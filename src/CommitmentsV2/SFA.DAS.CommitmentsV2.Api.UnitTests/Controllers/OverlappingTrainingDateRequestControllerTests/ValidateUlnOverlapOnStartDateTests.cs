using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
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
            var response = await _fixture.ValidateUlnOverlapOnStartDate();
            _fixture.VerifyQuerySent();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateUlnOverlapOnStartDate_HasOverlapWithIlrWithdrawnApprenticeship(bool HasOverlapWithIlrWithdrawnApprenticeship)
        {
            var response = await _fixture.withHasOverlapWithIlrWithdrawnApprenticeship(HasOverlapWithIlrWithdrawnApprenticeship).ValidateUlnOverlapOnStartDate();
            var result = response.Value as ValidateUlnOverlapOnStartDateResponse;

            result.Should().NotBeNull();
            result.HasOverlapWithIlrWithdrawnApprenticeship.Value.Should().Be(HasOverlapWithIlrWithdrawnApprenticeship);
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
            private ValidateUlnOverlapOnStartDateQueryResult queryResult;

            public ValidateUlnOverlapOnStartDateFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();

                Uln = "9235610906";
                StartDate = "Jan 2022";
                EndDate = "Dec 2022";

                queryResult = _autoFixture.Create<ValidateUlnOverlapOnStartDateQueryResult>();
                _mediator
                    .Setup(x => x.Send(It.IsAny<ValidateUlnOverlapOnStartDateQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(queryResult);

                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task<OkObjectResult> ValidateUlnOverlapOnStartDate()
            {
                var response = await _controller.ValidateUlnOverlapOnStartDate(ProviderId, Uln, StartDate, EndDate) as OkObjectResult;
                return response;
            }

            public ValidateUlnOverlapOnStartDateFixture withHasOverlapWithIlrWithdrawnApprenticeship(bool value)
            {
                queryResult.HasOverlapWithIlrWithdrawnApprenticeship = value;
                return this;
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