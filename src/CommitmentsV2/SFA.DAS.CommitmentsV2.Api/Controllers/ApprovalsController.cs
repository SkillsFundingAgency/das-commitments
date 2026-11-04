using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
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

        var incomingChanges = request.Changes;
        var command = await modelMapper.Map<CocApprovalCommand>(request);

        var result = await mediator.Send(command);
        if (result == null)
        {
            return Problem();
        }

        logger.LogInformation("PostApprovals completed Returning status of {Status}", result.Status);
        if (command.Action == AggregationAction.CancelPrevious)
        {
            return Ok(MapToApprovalsResult(new List<CocUpdateResult>(), request));
        }

        return Created("", MapToApprovalsResult(result.Items, request));
    }

    private static PutApprovalsResponse MapToApprovalsResult(List<CocUpdateResult> items, CocApprovalRequest request)
    {
        return new PutApprovalsResponse
        {
            Changes = MapToApprovalFieldChangeList(items, request.Changes).ToList(),
            Prices = request.Prices
        };
    }

    private static IEnumerable<ApprovalFieldChange> MapToApprovalFieldChangeList(List<CocUpdateResult> items, List<CocApprovalFieldChange> changes)
    {
        foreach (var item in items)
        {
            var changeType = item.Field.GetEnumDescription();
            yield return new ApprovalFieldChange
            {
                ChangeType = changeType,
                ApprovalStatus = GetApprovalStatus(item.Status),
                Reason = item.Reason,
                Data = ResolveData(item.Field, changeType, changes)
            };
        }

        foreach (var change in changes ?? [])
        {
            if (items.Any(x => x.Field.GetEnumDescription() == change.ChangeType))
            {
                continue;
            }

            yield return new ApprovalFieldChange
            {
                ChangeType = change.ChangeType,
                ApprovalStatus = CocApprovalItemStatus.AutoApproved.GetEnumDescription(),
                Reason = null,
                Data = IsPricesChange(change.ChangeType) ? null : change.Data
            };
        }
    }

    private static CocData ResolveData(CocChangeField field, string changeType, List<CocApprovalFieldChange> changes)
    {
        if (field == CocChangeField.Prices)
        {
            return null;
        }

        return changes?.FirstOrDefault(x => x.ChangeType == changeType)?.Data;
    }

    private static bool IsPricesChange(string changeType) =>
        string.Equals(changeType, nameof(CocChangeField.Prices), StringComparison.OrdinalIgnoreCase);

    private static string GetApprovalStatus(CocApprovalItemStatus status)
    {
        if (status == CocApprovalItemStatus.Pending)
        {
            return "EmployerApprovalRequested";
        }

        if (status == CocApprovalItemStatus.Reversal)
        {
            return CocApprovalItemStatus.AutoApproved.GetEnumDescription();
        }

        return status.GetEnumDescription();
    }
}
