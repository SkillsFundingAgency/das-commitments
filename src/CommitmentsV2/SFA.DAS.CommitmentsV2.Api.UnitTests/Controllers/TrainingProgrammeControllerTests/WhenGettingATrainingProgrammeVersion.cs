using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingATrainingProgrammeVersion
    {
        [Test, MoqAutoData]
        public async Task And_RequestIsValid_Then_ReturnTrainingProgramme(
            int courseCode,
            GetTrainingProgrammeVersionRequest request,
            [Frozen]Mock<IMediator> mediator,
            GetCalculatedTrainingProgrammeVersionQueryResult queryResult,
            TrainingProgrammeController controller)
        {
            mediator.Setup(m => m.Send(It.Is<GetCalculatedTrainingProgrammeVersionQuery>(q => q.CourseCode == courseCode && q.StartDate == request.StartDate), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            var response = await controller.GetCalculatedTrainingProgrammeVersion(courseCode, request) as OkObjectResult;

            var model = response.Value as GetTrainingProgrammeResponse;

            model.TrainingProgramme.Should().BeEquivalentTo(queryResult.TrainingProgramme);
        }

        [Test, MoqAutoData]
        public async Task And_StandardNotFound_Then_ReturnNotFound(
            int courseCode,
            GetTrainingProgrammeVersionRequest request,
            [Frozen] Mock<IMediator> mediator,
            TrainingProgrammeController controller)
        {
            mediator.Setup(m => m.Send(It.Is<GetCalculatedTrainingProgrammeVersionQuery>(q => q.CourseCode == courseCode && q.StartDate == request.StartDate), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCalculatedTrainingProgrammeVersionQueryResult());

            var response = await controller.GetCalculatedTrainingProgrammeVersion(courseCode, request) as NotFoundResult;

            response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Test, MoqAutoData]
        public async Task And_ThereIsAnError_Then_ReturnBadRequest(
            GetTrainingProgrammeVersionRequest request,
            [Frozen] Mock<IMediator> mediator,
            TrainingProgrammeController controller)
        {
            mediator.Setup(m => m.Send(It.IsAny<GetCalculatedTrainingProgrammeVersionQuery>(), It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            var response = await controller.GetCalculatedTrainingProgrammeVersion(1, request) as BadRequestResult;

            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}
