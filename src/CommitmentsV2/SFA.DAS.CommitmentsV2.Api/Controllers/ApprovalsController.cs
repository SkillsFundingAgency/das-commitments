using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Authorize]
[Route("approvals")]
[ApiController]
public class ApprovalsController(IMediator mediator, IModelMapper modelMapper, ILogger<ApprovalsController> logger) : ControllerBase
{
    [HttpPost("{learningKey}")]
    public async Task<ActionResult> PostApprovals([FromRoute] Guid learningKey, [FromBody] CocApprovalRequest request)
    {
        var details = await modelMapper.Map<CocApprovalDetails>(request);
        var result = await mediator.Send(new PostCocApprovalCommand { CocApprovalDetails = details });
        logger.LogInformation("PostApprovals completed Returning status of {0}", result?.Status);
        return Created("", MapToApprovalFieldChangeList(result.Items));
    }

    [HttpPut("{learningKey}")]
    public async Task<ActionResult> PutApprovals([FromRoute] Guid learningKey, [FromBody] CocApprovalRequest request)
    {
        var details = await modelMapper.Map<CocApprovalDetails>(request);
        var result = await mediator.Send(new PutCocApprovalCommand { CocApprovalDetails = details });
        logger.LogInformation("PutApprovals completed Returning status of {0}", result?.Status);
        return Created("", MapToApprovalFieldChangeList(result.Items));
    }
    
    private List<ApprovalFieldChange> MapToApprovalFieldChangeList(List<CocUpdateResult> items)
    {
        return items.Select(x => new ApprovalFieldChange
        {
            ChangeType = x.Field.GetEnumDescription(),
            ApprovalStatus = GetApprovalStatus(x.Status),
            Reason = x.Reason
        }).ToList();
    }

    private string GetApprovalStatus(CocApprovalItemStatus status)
    {
        if(status == CocApprovalItemStatus.Pending)
        {
            return "EmployerApprovalRequested";
        }
        return status.GetEnumDescription();
    }
}

public class ApprovalFieldChange
{
    public string ChangeType { get; set; }
    public string ApprovalStatus { get; set; }
    public string Reason { get; set; }
}

