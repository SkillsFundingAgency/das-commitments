using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/authorization")]
    [Authorize]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IApprenticeEmailFeatureService _apprenticeEmailFeatureService;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(IMediator mediator, IApprenticeEmailFeatureService apprenticeEmailFeatureService, ILogger<AuthorizationController> logger)
        {
            _mediator = mediator;
            _apprenticeEmailFeatureService = apprenticeEmailFeatureService;
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

        [HttpGet]
        [Route("features/providers/{providerId}/apprentice-email-required")]
        public IActionResult ApprenticeEmailRequired(long providerId)
        {
            _logger.LogInformation($"Check feature 'apprentice-email-required' is enabled for provider {providerId}");
            if(_apprenticeEmailFeatureService.IsEnabled && _apprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(providerId))
            {
                _logger.LogInformation($"Feature 'apprentice-email-required' is on for provider {providerId}");
                return Ok();
            }
            _logger.LogInformation($"Feature 'apprentice-email-required' is off for provider {providerId}");
            return NotFound();
        }

        [HttpGet]
        [Route("email-optional")]
        public async Task<IActionResult> OptionalEmail(long employerid, long providerId)
        {
            var query = new GetEmailOptionalQuery(employerid, providerId);

            bool resp = await _mediator.Send(query);

            if (resp)
                return Ok();
                
            return NotFound();
        }
    }
}