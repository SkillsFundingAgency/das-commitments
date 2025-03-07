﻿using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/authorization")]
[Authorize]
public class AuthorizationController(IMediator mediator, ILogger<AuthorizationController> logger)
    : ControllerBase
{
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

        return Ok(await mediator.Send(query));
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

        return Ok(await mediator.Send(query));
    }

    [HttpGet]
    [Route("features/providers/{providerId:long}/apprentice-email-required")]
    public async Task <IActionResult> ApprenticeEmailRequired(long providerId)
    {
        logger.LogInformation("Check feature 'apprentice-email-required' is enabled for provider {providerId}", providerId);
        var query = new GetEmailOptionalQuery(0, providerId);

        var resp = await mediator.Send(query);

        if (resp)
        {
            logger.LogInformation("Feature 'apprentice-email-required' is off for provider {providerId}", providerId);
            return NotFound();
        }

        logger.LogInformation("Feature 'apprentice-email-required' is on for provider {providerId}", providerId);
        return Ok();
    }

    [HttpGet]
    [Route("email-optional")]
    public async Task<IActionResult> OptionalEmail(long employerid, long providerId)
    {
        var query = new GetEmailOptionalQuery(employerid, providerId);

        var resp = await mediator.Send(query);

        if (resp)
        {
            return Ok();
        }

        return NotFound();
    }
}