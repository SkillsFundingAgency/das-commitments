using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("{AccountId}")]
        public async Task<IActionResult> GetAccount(long accountId)
        {
            var employer = await _mediator.Send(new GetAccountSummaryQuery
            {
                AccountId = accountId
            });

            return Ok(new AccountResponse
            {
                AccountId = employer.AccountId,
                HasApprenticeships = employer.HasApprenticeships,
                HasCohorts = employer.HasCohorts,
                LevyStatus = employer.LevyStatus
            });
        }

        [HttpGet]
        [Route("{AccountId}/providers/approved")]
        public async Task<IActionResult> GetApprovedProviders(long accountId)
        {
            var query = new GetApprovedProvidersQuery(accountId);

            var result = await _mediator.Send(query);

            return Ok(new GetApprovedProvidersResponse(result.ProviderIds));
        }
    }
}