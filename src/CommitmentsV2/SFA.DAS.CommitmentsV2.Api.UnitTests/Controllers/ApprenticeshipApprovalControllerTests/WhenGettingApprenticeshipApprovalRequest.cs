using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipApprovalControllerTests
{
    public class WhenGettingApprenticeshipApprovalRequest
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Data_Returned(
            GetApprenticeshipApprovalQueryResult result,
            long apprenticeshipId,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] ApprenticeshipApprovalsController controller)
        {
            mediator.Setup(x => x.Send(It.Is<GetApprenticeshipApprovalQuery>(q => q.ApprenticeshipId == apprenticeshipId),
                CancellationToken.None)).ReturnsAsync(result);

            var actual = await controller.GetApprenticeshipApproval(apprenticeshipId) as OkObjectResult;

            actual.Should().NotBeNull();
            var model = actual.Value as GetApprenticeshipApprovalQueryResult;
            model.Should().Be(result);
        }

        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_NoData_Returned(
            long apprenticeshipId,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] ApprenticeshipApprovalsController controller)
        {
            mediator.Setup(x => x.Send(It.Is<GetApprenticeshipApprovalQuery>(q => q.ApprenticeshipId == apprenticeshipId),
                CancellationToken.None)).ReturnsAsync((GetApprenticeshipApprovalQueryResult)null);

            var actual = await controller.GetApprenticeshipApproval(apprenticeshipId) as NotFoundResult;

            actual.Should().NotBeNull();
        }
    }
}
