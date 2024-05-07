using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TrainingProgrammeControllerTests
{
    public class WhenGettingATrainingProgramme
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Data_Returned(
            string id,
            GetTrainingProgrammeQueryResult result,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator.Setup(x => x.Send(It.Is<GetTrainingProgrammeQuery>(c=>c.Id.Equals(id)), CancellationToken.None)).ReturnsAsync(result);

            var actual = await controller.GetTrainingProgramme(id) as OkObjectResult;;

            //actual
            Assert.That(actual, Is.Not.Null);
            var model = actual.Value as GetTrainingProgrammeResponse;
            Assert.That(model, Is.Not.Null);
            model.TrainingProgramme.Should().BeEquivalentTo(result.TrainingProgramme);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Is_An_Error_A_Bad_Request_Is_Returned(
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrainingProgrammeQuery>(),
                    It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();
            
            var controllerResult = await controller.GetTrainingProgramme("1") as StatusCodeResult;

            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Course_Is_Not_Found_Then_A_NotFound_Result_Is_Returned(
            [Frozen] Mock<IMediator> mediator,
            [Greedy] TrainingProgrammeController controller)
        {
            mediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetTrainingProgrammeQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrainingProgrammeQueryResult
                {
                    TrainingProgramme = null
                });
            
            var controllerResult = await controller.GetTrainingProgramme("1") as StatusCodeResult;

            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}