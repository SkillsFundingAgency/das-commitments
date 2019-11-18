using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly IMediator _mediator;

        public AccountsController(
            ILogger<AccountsController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        [Route("{AccountId}")]
        public async Task<IActionResult> GetAccount(long accountId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.CreateErrorResponse());
            }

            var employer = await _mediator.Send(new GetAccountSummaryRequest
            {
                AccountId = accountId
            });

            if (employer == null)
            {
                return NotFound();
            }

            return Ok(new AccountResponse
            {
                AccountId = employer.AccountId,
                HasApprenticeships = employer.HasApprenticeships,
                HasCohorts = employer.HasCohorts
            });
        }
    }
}