using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/apprenticeships/{ApprenticeshipId:long}/approvals/{ApprovalRequestId:Guid}")]
public class ApprenticeshipApprovalsController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetApprenticeshipApproval(long apprenticeshipId, Guid approvalRequestId)
    {
        var result = await mediator.Send(new GetApprenticeshipApprovalQuery(apprenticeshipId, approvalRequestId));
        //var response = await modelMapper.Map<GetApprenticeshipApprovalResponse>(result);
        return Ok(null); // response);
    }
}