using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/authorization")]
    [Authorize]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IApprenticeEmailFeatureService _apprenticeEmailFeatureService;

        public AuthorizationController(IMediator mediator, IApprenticeEmailFeatureService apprenticeEmailFeatureService)
        {
            _mediator = mediator;
            _apprenticeEmailFeatureService = apprenticeEmailFeatureService;
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

        [HttpGet]
        [Route("features/providers/{providerId}/apprentice-email-required")]
        public IActionResult ApprenticeEmailRequired(long providerId)
        {
            if(_apprenticeEmailFeatureService.IsEnabled && _apprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(providerId))
            {
                return Ok();
            }
            return NotFound();
        }
    }
}