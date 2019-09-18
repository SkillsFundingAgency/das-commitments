using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.SendCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/cohorts")]
    public class CohortController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public CohortController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateCohortRequest request)
        {
            var command = new AddCohortCommand(
                request.AccountId,
                request.AccountLegalEntityId,
                request.ProviderId,
                request.CourseCode,
                request.Cost,
                request.StartDate,
                request.EndDate,
                request.OriginatorReference,
                request.ReservationId,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Uln,
                request.UserInfo);
            
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }

        [HttpPost]
        [Route("with-other-party")] // TODO: Remove after CV-388 has been deployed to PROD
        [Route("create-with-other-party")]
        public async Task<IActionResult> Create([FromBody]CreateCohortWithOtherPartyRequest request)
        {
            var command = new AddCohortWithOtherPartyCommand(request.AccountId, request.AccountLegalEntityId, request.ProviderId, request.Message, request.UserInfo);
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }

        [HttpGet]
        [Route("{cohortId}")]
        public async Task<IActionResult> Get(long cohortId)
        {
            var query = new GetCohortSummaryQuery(cohortId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound();
            }
            
            return Ok(new GetCohortResponse
            {
                CohortId = result.CohortId,
                AccountLegalEntityId = result.AccountLegalEntityId,
                LegalEntityName = result.LegalEntityName,
                ProviderName = result.ProviderName,
                TransferSenderId = result.TransferSenderId,
                WithParty = result.WithParty,
                LatestMessageCreatedByEmployer = result.LatestMessageCreatedByEmployer,
                LatestMessageCreatedByProvider = result.LatestMessageCreatedByProvider,
                EditStatus = result.EditStatus
            });
        }

        [HttpPost]
        [Route("{cohortId}/send")]
        public async Task<IActionResult> Send(long cohortId, [FromBody]SendCohortRequest request)
        {
            var command = new SendCohortCommand(cohortId, request.Message, request.UserInfo);
            await _mediator.Send(command);
            
            return Ok();
        }
    }
}