using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingAllStandards
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Data_Returned(
            GetAllTrainingProgrammeStandardsQueryResult result,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator.Setup(x => x.Send(It.IsAny<GetAllTrainingProgrammeStandardsQuery>(), CancellationToken.None)).ReturnsAsync(result);

            var actual = await controller.GetAllStandards() as OkObjectResult;;

            //actual
            Assert.That(actual, Is.Not.Null);
            var model = actual.Value as GetAllTrainingProgrammeStandardsResponse;
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
                    It.IsAny<GetAllTrainingProgrammeStandardsQuery>(),
                    It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();
            
            var controllerResult = await controller.GetAllStandards() as StatusCodeResult;

            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
        
    }
}