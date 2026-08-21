using Microsoft.AspNetCore.Authorization;
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
}