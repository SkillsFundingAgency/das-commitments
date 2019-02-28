using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Queries.GetAccountLegalEntity;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountLegalEntityController : ControllerBase
    {
        private readonly ILogger<AccountLegalEntityController> _logger;
        private readonly IMediator _mediator;

        public AccountLegalEntityController(
            ILogger<AccountLegalEntityController> logger, 
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [Authorize(Policies.Provider)]
        [HttpGet]
        [Route("{AccountLegalEntityId}")]
        public async Task<IActionResult> GetAccountLegalEntity(long accountLegalEntityId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employer = await _mediator.Send(new GetAccountLegalEntityRequest
            {
                AccountLegalEntityId = accountLegalEntityId
            });

            if (employer == null)
            {
                return NotFound();
            }

            return Ok(new AccountLegalEntity
            {
                AccountName = employer.AccountName,
                LegalEntityName = employer.LegalEntityName
            });
        }
    }
}
