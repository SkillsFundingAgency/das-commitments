using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;
using SFA.DAS.CommitmentsV2.Application.Commands.TriageDataLocks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/apprenticeships/{apprenticeshipId:long}")]
public class DataLocksController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpGet("datalocks")]
    public async Task<IActionResult> GetDataLocks(long apprenticeshipId)
    {
        var result = await mediator.Send(new GetDataLocksQuery(apprenticeshipId));
        var response = await modelMapper.Map<GetDataLocksResponse>(result);

        return Ok(response);
    }

    [HttpGet("datalocksummaries")]
    public async Task<IActionResult> GetDataLockSummaries(long apprenticeshipId)
    {
        var result = await mediator.Send(new GetDataLockSummariesQuery(apprenticeshipId));
        var response = await modelMapper.Map<GetDataLockSummariesResponse>(result);
        
        return Ok(response);
    }

    [HttpPost]
    [Route("datalocks/accept-changes")]
    public async Task<IActionResult> AcceptDataLockChanges(long apprenticeshipId, [FromBody] AcceptDataLocksRequestChangesRequest request)
    {
        await mediator.Send(new AcceptDataLocksRequestChangesCommand(
            apprenticeshipId,
            request.UserInfo));

        return Ok();
    }

    [HttpPost]
    [Route("datalocks/reject-changes")]
    public async Task<IActionResult> RejectDataLockChanges(long apprenticeshipId, [FromBody] RejectDataLocksRequestChangesRequest request)
    {
        await mediator.Send(new RejectDataLocksRequestChangesCommand(
            apprenticeshipId,
            request.UserInfo));

        return Ok();
    }

    [HttpPost]
    [Route("datalocks/triage")]
    public async Task<IActionResult> TriageDataLocks(long apprenticeshipId, [FromBody] TriageDataLocksRequest request)
    {
        await mediator.Send(new TriageDataLocksCommand(
            apprenticeshipId,
            request.TriageStatus,
            request.UserInfo));

        return Ok();
    }
}