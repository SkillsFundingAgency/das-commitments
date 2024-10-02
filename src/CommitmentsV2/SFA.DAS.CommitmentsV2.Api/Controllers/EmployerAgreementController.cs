using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/employer-agreements/{AccountLegalEntityId:long}")]
[ApiController]
[Authorize]
public class EmployerAgreementController(
    IMediator mediator,
    IEmployerAgreementService employerAgreementService)
    : ControllerBase
{
    [HttpGet]
    [Route("signed")]
    public async Task<IActionResult> IsAgreementSignedForFeature(long accountLegalEntityId, [FromQuery] AgreementFeature[] agreementFeatures, CancellationToken cancellationToken)
    {
        var accountLegalEntity = await mediator.Send(new GetAccountLegalEntityQuery{AccountLegalEntityId = accountLegalEntityId}, cancellationToken);
        var isSigned = await employerAgreementService.IsAgreementSigned(accountLegalEntity.AccountId,
            accountLegalEntity.MaLegalEntityId, agreementFeatures);

        return Ok(isSigned);
    }

    [HttpGet]
    [Route("latest-id")]
    public async Task<IActionResult> GetLatestAgreementId(long accountLegalEntityId, CancellationToken cancellationToken)
    {
        var accountLegalEntity = await mediator.Send(new GetAccountLegalEntityQuery { AccountLegalEntityId = accountLegalEntityId }, cancellationToken);
        var agreementId = await employerAgreementService.GetLatestAgreementId(accountLegalEntity.AccountId, accountLegalEntity.MaLegalEntityId);

        return Ok(agreementId);
    }
}