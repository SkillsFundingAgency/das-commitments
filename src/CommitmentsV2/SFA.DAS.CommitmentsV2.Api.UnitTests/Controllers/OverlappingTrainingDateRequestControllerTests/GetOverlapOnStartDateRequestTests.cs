using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.OverlappingTrainingDateRequestControllerTests
{
    public class GetOverlapOnStartDateRequestTests
    {
        private GetOverlapOnStartDateTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetOverlapOnStartDateTestsFixture();
        }

        [Test]
        public async Task GetOverlapOnStartDate_VerifyQuerySent()
        {
            await _fixture.Get();
            _fixture.VerifyQuerySent();
        }

        [Test]
        public async Task GetForNoOverlapOnStartDateRequest_VerifyResponse()
        {
            await _fixture.GetForNoOverlapOnStartDateRequest();
            _fixture.VerifyNotFoundReturnNull();
        }

        private class GetOverlapOnStartDateTestsFixture
        {
            private readonly Mock<IMediator> _mediator;
            private OverlappingTrainingDateRequestController _controller;
            private Mock<IModelMapper> _mapper;
            private readonly Fixture _autoFixture;
            private const long ApprenticeshipId = 1;
            private ActionResult _actionResult;

            public GetOverlapOnStartDateTestsFixture()
            {
                _mediator = new Mock<IMediator>();
                _mapper = new Mock<IModelMapper>();
                _autoFixture = new Fixture();

                var queryResult = _autoFixture.Create<GetOverlappingTrainingDateRequestQueryResult>();

                _mediator
                    .Setup(x => x.Send(It.IsAny<GetOverlappingTrainingDateRequestQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(queryResult);

                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);
            }

            public async Task Get()
            {
                _actionResult = await _controller.Get(ApprenticeshipId) as ActionResult;
            }

            public async Task GetForNoOverlapOnStartDateRequest()
            {
                _mediator
                 .Setup(x => x.Send(It.IsAny<GetOverlappingTrainingDateRequestQuery>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.FromResult(default(GetOverlappingTrainingDateRequestQueryResult)));

                _controller = new OverlappingTrainingDateRequestController(_mediator.Object, _mapper.Object);

                _actionResult = await _controller.Get(ApprenticeshipId) as ActionResult;
            }

            public void VerifyQuerySent()
            {
                _mediator.Verify(m => m.Send(It.Is<GetOverlappingTrainingDateRequestQuery>(p => p.ApprenticeshipId == ApprenticeshipId),
                    It.IsAny<CancellationToken>()), Times.Once);
            }

            public void VerifyNotFoundReturnNull()
            {
                var vm = _actionResult as ViewResult;
                //Assert

                Assert.IsNull(vm);
            }
        }
    }
}