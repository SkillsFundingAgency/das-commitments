using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/authorization")]
    [Authorize]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(IMediator mediator, ILogger<AuthorizationController> logger)
        {
            _mediator = mediator;            
            _logger = logger;
        }

        [HttpGet]
        [Route("access-cohort")]
        public async Task<IActionResult> CanAccessCohort(CohortAccessRequest request)
        {
            var query = new CanAccessCohortQuery
            {
                CohortId = request.CohortId,
                Party = request.Party,
                PartyId = request.PartyId
            };

            return Ok(await _mediator.Send(query));
        }

        [HttpGet]
        [Route("access-apprenticeship")]
        public async Task<IActionResult> CanAccessApprenticeship(ApprenticeshipAccessRequest request)
        {
            var query = new CanAccessApprenticeshipQuery
            {
                ApprenticeshipId = request.ApprenticeshipId,
                Party = request.Party,
                PartyId = request.PartyId
            };

            return Ok(await _mediator.Send(query));
        }
    }
}