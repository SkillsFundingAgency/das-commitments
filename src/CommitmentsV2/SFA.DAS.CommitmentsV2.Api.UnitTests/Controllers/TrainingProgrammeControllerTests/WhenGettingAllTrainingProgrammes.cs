using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingAllTrainingProgrammes
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Data_Returned(
            GetAllTrainingProgrammesQueryResult result,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator.Setup(x => x.Send(It.IsAny<GetAllTrainingProgrammesQuery>(), CancellationToken.None)).ReturnsAsync(result);

            var actual = await controller.GetAll() as OkObjectResult;;

            //actual
            Assert.That(actual, Is.Not.Null);
            var model = actual.Value as GetAllTrainingProgrammesResponse;
            Assert.That(model, Is.Not.Null);
            model.TrainingProgrammes.Should().BeEquivalentTo(result.TrainingProgrammes);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Is_An_Error_A_Bad_Request_Is_Returned(
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetAllTrainingProgrammesQuery>(),
                    It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();
            
            var controllerResult = await controller.GetAll() as StatusCodeResult;

            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}