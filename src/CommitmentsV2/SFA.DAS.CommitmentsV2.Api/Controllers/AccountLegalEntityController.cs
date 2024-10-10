using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountLegalEntityController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpGet]
    [Route("{AccountLegalEntityId:long}")]
    public async Task<IActionResult> GetAccountLegalEntity(long accountLegalEntityId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState.CreateErrorResponse());
        }

        var employer = await mediator.Send(new GetAccountLegalEntityQuery
        {
            AccountLegalEntityId = accountLegalEntityId
        });

        if (employer == null)
        {
            return NotFound();
        }

        return Ok(new AccountLegalEntityResponse
        {
            AccountId = employer.AccountId,
            MaLegalEntityId = employer.MaLegalEntityId,
            AccountName = employer.AccountName,
            LegalEntityName = employer.LegalEntityName,
            LevyStatus = employer.LevyStatus
        });
    }
}