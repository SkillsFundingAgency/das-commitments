﻿using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetStandardOptions;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingStandardOptions
    {
        private Mock<IMediator> _mockMediator;

        private TrainingProgrammeController _controller;

        [SetUp]
        public void Arrange()
        {
            _mockMediator = new Mock<IMediator>();

            _controller = new TrainingProgrammeController(_mockMediator.Object, Mock.Of<ILogger<TrainingProgrammeController>>());
        }

        [Test, MoqAutoData]
        public async Task Then_GetStandardOptionsMediatorQueryIsCalled(string standardUId, GetStandardOptionsResult result)
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetStandardOptionsQuery>(), CancellationToken.None)).ReturnsAsync(result);

            await _controller.GetStandardOptions(standardUId);

            _mockMediator.Verify(m => m.Send(It.Is<GetStandardOptionsQuery>(q => q.StandardUId == standardUId), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test, MoqAutoData]
        public async Task Then_GetStandardOptionsResultIsReturned(string standardUId, GetStandardOptionsResult result)
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetStandardOptionsQuery>(), CancellationToken.None)).ReturnsAsync(result);

            var controllerResponse = await _controller.GetStandardOptions(standardUId) as OkObjectResult;

            controllerResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var model = controllerResponse.Value as GetStandardOptionsResponse;

            model.Options.Should().BeEquivalentTo(result.Options);
        }

        [Test, MoqAutoData]
        public async Task And_MediatorThrowsException_Then_ReturnBadRequest(string standardUId)
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetStandardOptionsQuery>(), CancellationToken.None)).Throws<Exception>();

            var response = await _controller.GetStandardOptions(standardUId) as BadRequestResult;

            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}