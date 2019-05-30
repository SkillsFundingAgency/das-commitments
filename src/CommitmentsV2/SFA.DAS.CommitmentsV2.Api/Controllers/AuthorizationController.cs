using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/authorization")]
    [Authorize]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthorizationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("access-cohort")]
        public async Task<IActionResult> CanAccessCohort(PartyType partyType, string partyId, long cohortId)
        {
            var query = MapToCanAccessCohortQuery(partyType, partyId, cohortId);

            return Ok(await _mediator.Send(query));
        }

        private CanAccessCohortQuery MapToCanAccessCohortQuery(PartyType partyType, string partyId, long cohortId)
        {
            var query = new CanAccessCohortQuery
            {
                PartyType = partyType,
                CohortId = cohortId
            };

            switch (partyType)
            {
                case PartyType.Employer when long.TryParse(partyId, out long accountId):
                {
                    query.AccountId = accountId;
                    break;
                }
                case PartyType.Provider when long.TryParse(partyId, out long providerId):
                {
                    query.ProviderId = providerId;
                    break;
                }
            }

            return query;
        }
    }
}