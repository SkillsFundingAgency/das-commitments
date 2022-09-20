using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlapRequests;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class GetRequestTests
    {
        private GetRequestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetRequestFixture();
        }

        [Test]
        public async Task ValidateUlnOverlapOnStartDate_VerifyQuerySent()
        {
            await _fixture.GetRequest();
            _fixture.VerifyQuerySent();
        }

        private class GetRequestFixture
        {
            private readonly Mock<IMediator> _mediator;
            private readonly OverlappingTrainingDateRequestController _controller;

            private readonly Fixture _autoFixture;
            public const int ApprenticeshipId = 1;

            public GetRequestFixture()
            {
                _mediator = new Mock<IMediator>();
                _autoFixture = new Fixture();

                var queryResult = _autoFixture.Create<GetOverlapRequestsQueryResult>();
                _mediator
                    .Setup(x => x.Send(It.IsAny<GetOverlapRequestsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(queryResult);

                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, Mock.Of<IModelMapper>());
            }

            public async Task GetRequest()
            {
                await _controller.GetRequest(ApprenticeshipId);
            }

            public void VerifyQuerySent()
            {
                _mediator.Verify(
                    m => m.Send(
                        It.Is<GetOverlapRequestsQuery>(p => p.DraftApprenticeshipId == ApprenticeshipId),
                        It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}