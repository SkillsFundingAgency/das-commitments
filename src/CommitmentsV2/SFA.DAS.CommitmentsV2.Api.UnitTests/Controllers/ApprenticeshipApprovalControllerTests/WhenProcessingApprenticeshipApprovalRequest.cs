using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipApprovalControllerTests
{
    public class WhenProcessingApprenticeshipApprovalRequest
    {
        [Test, MoqAutoData]
        public async Task Then_The_Request_Is_Passed_To_Mediator_And_Ok_Returned(
            long apprenticeshipId,
            Guid ApprovalRequestId,
            ProcessApprenticeshipApprovalRequest request,
            [Frozen] Mock<IMediator> mediator,
            [Greedy] ApprenticeshipApprovalsController controller)
        {
            mediator.Setup(x => x.Send(It.IsAny<ProcessApprenticeshipApprovalCommand>(), CancellationToken.None));

            var actual = await controller.PostApprenticeshipApproval(apprenticeshipId, ApprovalRequestId, request) as OkResult;

            actual.Should().NotBeNull();
            mediator.Verify(x => x.Send(It.Is<ProcessApprenticeshipApprovalCommand>(c =>
                c.ApprenticeshipId == apprenticeshipId &&
                c.ApprovalRequestId == ApprovalRequestId &&
                c.ApplyChanges == request.ApplyChanges &&
                c.UserInfo == request.UserInfo), CancellationToken.None), Times.Once);
        }
    }
}