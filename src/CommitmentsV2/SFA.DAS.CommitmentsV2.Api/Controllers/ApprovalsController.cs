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
    [HttpPut("{learningKey}")]
    public async Task<ActionResult> PutApprovals([FromRoute] Guid learningKey, [FromBody] CocApprovalRequest request)
    {
        if (learningKey != request.LearningKey)
        {
            return BadRequest("LearningKey in route does not match LearningKey in body");
        }

        var command = await modelMapper.Map<CocApprovalCommand>(request);

        var result = await mediator.Send(command);
        logger.LogInformation("PostApprovals completed Returning status of {0}", result?.Status);
        if (command.Action == AggregrationAction.CancelPrevious)
        {
            return Ok(MapToApprovalFieldChangeList(new List<CocUpdateResult>(), request.Changes).ToList());
        }
            
        return Created("", MapToApprovalFieldChangeList(result.Items, request.Changes).ToList());
    }

    private IEnumerable<ApprovalFieldChange> MapToApprovalFieldChangeList(List<CocUpdateResult> items, List<CocApprovalFieldChange> changes)
    {
        foreach(var item in items)
        {
            yield return new ApprovalFieldChange
            {
                ChangeType = item.Field.GetEnumDescription(),
                ApprovalStatus = GetApprovalStatus(item.Status),
                Reason = item.Reason
            };
        }

        foreach (var change in changes)
        {
            if (items.Any(x => x.Field.GetEnumDescription() == change.ChangeType))
            {
                continue;
            }
            yield return new ApprovalFieldChange
            {
                ChangeType = change.ChangeType,
                ApprovalStatus = CocApprovalItemStatus.AutoApproved.GetEnumDescription(),
                Reason = null
            };
        }
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

