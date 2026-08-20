using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/apprenticeships/{ApprenticeshipId:long}/approvals/{ApprovalRequestId:Guid}")]
public class ApprenticeshipApprovalsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetApprenticeshipApproval(long apprenticeshipId, Guid approvalRequestId)
    {
        var result = await mediator.Send(new GetApprenticeshipApprovalQuery(apprenticeshipId, approvalRequestId));

        if(result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> PostApprenticeshipApproval(long apprenticeshipId, Guid approvalRequestId, [FromBody] ProcessApprenticeshipApprovalRequest request)
    {
        await mediator.Send(new ProcessApprenticeshipApprovalCommand
        {
            ApprenticeshipId = apprenticeshipId,
            ApprovalRequestId = approvalRequestId,
            ApplyChanges = request.ApplyChanges,
            UserInfo = request.UserInfo
        });

        return Ok();
    }
}