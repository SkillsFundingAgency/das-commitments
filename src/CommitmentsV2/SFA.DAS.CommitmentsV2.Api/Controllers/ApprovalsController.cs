using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.CocDelete;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("approvals")]
[ApiController]
public class ApprovalsController(IMediator mediator, IModelMapper modelMapper, ILogger<ApprovalsController> logger) : ControllerBase
{
    [Authorize]
    [HttpPost("{learningKey}")]
    public async Task<ActionResult> PostApprovals([FromRoute] Guid learningKey, [FromBody] CocApprovalRequest request)
    {
        var command = await modelMapper.Map<PostCocApprovalCommand>(request);
        var result = await mediator.Send(command);
        logger.LogInformation("=== COMMITMENTS API: ApprovalsController.PostApprovals completed === Returning status of {0}", result?.Status);
        return Created("", result.Items.Select(x => new
        {
            ChangeType = x.Field.GetEnumDescription(),
            ApprovalStatus = x.Status.GetEnumDescription(),
            x.Reason
        }).ToList());
    }

    [Authorize]
    [HttpDelete("{learningKey}")]
    public async Task<ActionResult> DeleteApprovals([FromRoute] Guid learningKey)
    {
        var command = new CocDeleteCommand { LearningKey = learningKey };
        var result = await mediator.Send(command);

        return result.Status switch
        {
            DeleteValidationState.Cancelled => Ok(result.Message),
            DeleteValidationState.NotFound => NotFound(result.Message),
            DeleteValidationState.NotPending => BadRequest(result.Message),
            _ => StatusCode((int)result.Status, result.Message)
        };
    }
}