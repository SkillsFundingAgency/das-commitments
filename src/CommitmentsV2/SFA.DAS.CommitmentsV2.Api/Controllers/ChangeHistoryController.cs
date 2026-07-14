using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistoryForEmployer;

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

        logger.LogInformation("Successfully retrieved change history for apprenticeship with id {ApprenticeshipId}", apprenticeshipId);

        return Ok(new GetChangeHistoryResponse { ChangeHistory = result.ChangeHistory });
    }

    [Authorize]
    [HttpGet]
    [Route("employer/{accountId:long}/change-history")]
    public async Task<IActionResult> GetChangeHistoryForEmployer(long accountId)
    {
        logger.LogInformation("Received request to get change history for all learners of employer with id {accountId}", accountId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.CreateErrorResponse());
        }

        var result = await mediator.Send(new GetChangeHistoryForEmployerQuery
        {
            AccountId = accountId
        });

        logger.LogInformation("Successfully retrieved change history for all learners of employer with id {AccountId}", accountId);

        return Ok(new GetChangeHistoryForEmployerResponse { ChangeHistory = result.ChangeHistory });
    }
}