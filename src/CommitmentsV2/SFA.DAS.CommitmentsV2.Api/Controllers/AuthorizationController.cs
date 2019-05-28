using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthorizationController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("access-cohort")]
        public async Task<IActionResult> CanAccessCohort(CohortAccessRequest request)
        {
            var cohort = await _mediator.Send(new GetCohortSummaryRequest{CohortId = request.CohortId});

            return Ok(PartyCanAccessCohort(request.PartyType, request.PartyId, cohort));
        }

        private bool PartyCanAccessCohort(PartyType partyType, string partyId, GetCohortSummaryResponse cohort)
        {
            switch (partyType)
            {
                case PartyType.Employer when long.TryParse(partyId, out long accountId):
                {
                    return accountId == cohort.AccountId;
                }
                case PartyType.Provider when long.TryParse(partyId, out long providerId):
                {
                    return providerId == cohort.ProviderId;
                }
            }
            return false;
        }
    }
}