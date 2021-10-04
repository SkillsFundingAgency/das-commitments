using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingTrainingProgrammeVersions
    {
        private GetTrainingProgrammeVersionsQuery _query;
        private GetTrainingProgrammeVersionsQueryResult _queryResult;
        private Mock<IMediator> _mockMediator; 
        private TrainingProgrammeController _controller;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();

            _query = new GetTrainingProgrammeVersionsQuery(fixture.Create<int>().ToString());
            _queryResult = fixture.Create<GetTrainingProgrammeVersionsQueryResult>();

            _mockMediator = new Mock<IMediator>();

            _controller = new TrainingProgrammeController(_mockMediator.Object, Mock.Of<ILogger<TrainingProgrammeController>>());
        }

        [Test]
        public async Task Then_RequestIsPassedToMediator_And_DataIsReturned()
        {
            _mockMediator.Setup(m => m.Send(It.Is<GetTrainingProgrammeVersionsQuery>(q => q.Id == _query.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_queryResult);

            var result = await _controller.GetTrainingProgrammeVersions(_query.Id) as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var model = result.Value as GetTrainingProgrammeVersionsResponse;

            model.TrainingProgrammeVersions.Should().BeEquivalentTo(_queryResult.TrainingProgrammes);
        }

        [Test]
        public async Task And_MediatorThrowsException_Then_ReturnBadRequest()
        {
            _mockMediator.Setup(m => m.Send(It.Is<GetTrainingProgrammeVersionsQuery>(q => q.Id == _query.Id), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var result = await _controller.GetTrainingProgrammeVersions(_query.Id) as BadRequestResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task And_MediatorReturnNoResults_Then_ReturnNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.Is<GetTrainingProgrammeVersionsQuery>(q => q.Id == _query.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrainingProgrammeVersionsQueryResult { TrainingProgrammes = null});

            var result = await _controller.GetTrainingProgrammeVersions(_query.Id) as NotFoundResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
