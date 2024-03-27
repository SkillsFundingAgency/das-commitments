using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/employer-agreements")]
    [ApiController]
    [Authorize]
    public class EmployerAgreementController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEmployerAgreementService _employerAgreementService;
 
        public EmployerAgreementController(IMediator mediator,
            IEmployerAgreementService employerAgreementService)
        {
            _mediator = mediator;
            _employerAgreementService = employerAgreementService;
        }

        [HttpGet]
        [Route("{AccountLegalEntityId}/signed")]
        public async Task<IActionResult> IsAgreementSignedForFeature(long accountLegalEntityId, [FromQuery] AgreementFeature[] agreementFeatures, CancellationToken cancellationToken)
        {
            var accountLegalEntity = await _mediator.Send(new GetAccountLegalEntityQuery{AccountLegalEntityId = accountLegalEntityId}, cancellationToken);
            var isSigned = await _employerAgreementService.IsAgreementSigned(accountLegalEntity.AccountId,
                accountLegalEntity.MaLegalEntityId, agreementFeatures);

            return Ok(isSigned);
        }

        [HttpGet]
        [Route("{AccountLegalEntityId}/latest-id")]
        public async Task<IActionResult> GetLatestAgreementId(long accountLegalEntityId, CancellationToken cancellationToken)
        {
            var accountLegalEntity = await _mediator.Send(new GetAccountLegalEntityQuery { AccountLegalEntityId = accountLegalEntityId }, cancellationToken);
            var agreementId = await _employerAgreementService.GetLatestAgreementId(accountLegalEntity.AccountId, accountLegalEntity.MaLegalEntityId);

            return Ok(agreementId);
        }
    }
}
