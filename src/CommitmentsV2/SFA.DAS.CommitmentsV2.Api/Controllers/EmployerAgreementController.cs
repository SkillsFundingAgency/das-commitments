using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/employer-agreement")]
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
        [Route("{AccountLegalEntityId}/features-signed")]
        public async Task<IActionResult> IsAgreementSignedForFeature(AgreementSignedRequest request)
        {
            var accountLegalEntity = await _mediator.Send(new GetAccountLegalEntityRequest
                {AccountLegalEntityId = request.AccountLegalEntityId});
            var isSigned = await _employerAgreementService.IsAgreementSigned(accountLegalEntity.AccountId,
                accountLegalEntity.MaLegalEntityId, request.AgreementFeatures);

            return Ok(isSigned);
        }

        [HttpGet]
        [Route("{AccountLegalEntityId}/latest-id")]
        public async Task<IActionResult> GetLatestAgreementId(long accountLegalEntityId)
        {
            var accountLegalEntity = await _mediator.Send(new GetAccountLegalEntityRequest { AccountLegalEntityId = accountLegalEntityId });
            var agreementId = await _employerAgreementService.GetLatestAgreementId(accountLegalEntity.AccountId, accountLegalEntity.MaLegalEntityId);

            return Ok(agreementId);
        }
    }
}
