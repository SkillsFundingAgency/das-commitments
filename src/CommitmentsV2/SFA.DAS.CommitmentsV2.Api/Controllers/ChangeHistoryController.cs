using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Authorize]
[Route("api/change-history")]
[ApiController]
public class ChangeHistoryController(IMediator mediator, ILogger<ChangeHistoryController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet]
    [Route("{ApprenticeshipId:long}")]
    public async Task<IActionResult> GetChangeHistory(long apprenticeshipId)
    {
        logger.LogInformation("Received request to get change history for apprenticeship with id {ApprenticeshipId}", apprenticeshipId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.CreateErrorResponse());
        }

        var result = await mediator.Send(new GetChangeHistoryQuery
        {
            ApprenticeshipId = apprenticeshipId
        });

        if (result == null)
        {
            return NotFound();
        }

        logger.LogInformation("Successfully retrieved change history for apprenticeship with id {ApprenticeshipId}", apprenticeshipId);

        return Ok(new GetChangeHistoryResponse { ChangeHistory = result.ChangeHistory });
    }
}