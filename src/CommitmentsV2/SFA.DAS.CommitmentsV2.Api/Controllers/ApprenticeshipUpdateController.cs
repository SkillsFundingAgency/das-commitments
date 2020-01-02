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
    [Route("api/apprenticeships/{ApprenticeshipId}")]
    [ApiController]
    [Authorize]
    public class ApprenticeshipUpdateController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApprenticeshipUpdateController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("/pending-update")]
        public async Task<IActionResult> GetPendingUpdate(long accountId)
        {
            var employer = await _mediator.Send(new GetAccountSummaryRequest
            {
                AccountId = accountId
            });

            return Ok(new AccountResponse
            {
                AccountId = employer.AccountId,
                HasApprenticeships = employer.HasApprenticeships,
                HasCohorts = employer.HasCohorts
            });
        }
    }
}