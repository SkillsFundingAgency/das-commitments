using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingNewerStandardVersions
    {
        [Test, MoqAutoData]
        public async Task Then_DataIsReturned(
            string standardUId,
            GetNewerTrainingProgrammeVersionsQueryResult queryResult,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator.Setup(x => x.Send(It.IsAny<GetNewerTrainingProgrammeVersionsQuery>(), CancellationToken.None))
                .ReturnsAsync(queryResult);

            var result = await controller.GetNewerTrainingProgrammeVersions(standardUId) as OkObjectResult;

            Assert.That(result, Is.Not.Null);

            var model = result.Value as GetNewerTrainingProgrammeVersionsResponse;

            model.NewerVersions.Should().BeEquivalentTo(queryResult.NewerVersions);
        }

        [Test, MoqAutoData]
        public async Task And_ThereIsAnError_Then_ReturnBadRequest(
            string standardUId,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetNewerTrainingProgrammeVersionsQuery>(),
                    It.IsAny<CancellationToken>()))
                .Throws<Exception>();

            var controllerResult = await controller.GetNewerTrainingProgrammeVersions(standardUId) as StatusCodeResult;

            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}
