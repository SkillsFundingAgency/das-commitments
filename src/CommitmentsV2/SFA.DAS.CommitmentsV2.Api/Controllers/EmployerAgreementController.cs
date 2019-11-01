using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmployerAgreementController> _logger;
 
        public EmployerAgreementController(IMediator mediator,
            IEmployerAgreementService employerAgreementService,
            ILogger<EmployerAgreementController> logger)
        {
            _mediator = mediator;
            _employerAgreementService = employerAgreementService;
            _logger = logger;
        }

        [HttpGet]
        [Route("{AccountLegalEntityId}/feature-signed")]
        public async Task<IActionResult> IsAgreementSignedForFeature(AgreementSignedRequest request)
        {
            var accountLegalEntity = await _mediator.Send(new GetAccountLegalEntityRequest { AccountLegalEntityId = request.AccountLegalEntityId });
            var isSigned = _employerAgreementService.IsAgreementSigned(accountLegalEntity.AccountId,
                accountLegalEntity.MaLegalEntityId, request.AgreementFeatures);

            return Ok(isSigned);
        }

        [HttpGet]
        [Route("{AccountLegalEntityId}/latest")]
        public async Task<IActionResult> GetLatestAgreementId(long accountLegalEntityId)
        {
            var accountLegalEntity = await _mediator.Send(new GetAccountLegalEntityRequest { AccountLegalEntityId = accountLegalEntityId });
            var agreementId = _employerAgreementService.GetLatestAgreementId(accountLegalEntity.AccountId, accountLegalEntity.MaLegalEntityId);

            return Ok(agreementId);
        }
    }
}
