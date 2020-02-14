﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.ApproveCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.SendCohort;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;
using SFA.DAS.CommitmentsV2.Types;


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

        [HttpGet]
        public async Task<IActionResult> GetCohorts([FromQuery] GetCohortsRequest request)
        {
            var query = new GetCohortsQuery(request.AccountId);
            var result = await _mediator.Send(query);

            return Ok(new GetCohortsResponse(result.Cohorts));
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
                request.TransferSenderId,
                request.UserInfo);
            
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }

        [HttpPost]
        [Route("create-with-other-party")]
        public async Task<IActionResult> Create([FromBody]CreateCohortWithOtherPartyRequest request)
        {
            var command = new AddCohortWithOtherPartyCommand(request.AccountId, request.AccountLegalEntityId, request.ProviderId, request.TransferSenderId, request.Message, request.UserInfo);
            var result = await _mediator.Send(command);

            return Ok(new CreateCohortResponse
            {
                CohortId = result.Id,
                CohortReference = result.Reference
            });
        }

        [HttpPost]
        [Route("create-empty-cohort")]
        public async Task<IActionResult> Create([FromBody]CreateEmptyCohortRequest request)
        {
            var command = new AddEmptyCohortCommand(request.AccountId, request.AccountLegalEntityId, request.ProviderId, request.UserInfo);
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
                IsApprovedByEmployer = result.IsApprovedByEmployer,
                IsApprovedByProvider = result.IsApprovedByProvider,
                IsCompleteForEmployer = result.IsCompleteForEmployer
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

        [HttpPost]
        [Route("{cohortId}/approve")]
        public async Task<IActionResult> Approve(long cohortId, [FromBody]ApproveCohortRequest request)
        {
            var command = new ApproveCohortCommand(cohortId, request.Message, request.UserInfo);
            await _mediator.Send(command);

            return Ok();
        } 
        
        [HttpPost]
        [Route("{cohortId}/delete")]
        public async Task<IActionResult> Delete(long cohortId, [FromBody]UserInfo userInfo, CancellationToken cancellationToken)
        {
            var command = new DeleteCohortCommand { CohortId = cohortId, UserInfo = userInfo };
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}