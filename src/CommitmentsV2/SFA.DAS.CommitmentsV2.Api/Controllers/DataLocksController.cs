using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;
using SFA.DAS.CommitmentsV2.Application.Commands.TriageDataLocks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLockSummaries;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/")]
    public class DataLocksController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public DataLocksController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet("{apprenticeshipId}/datalocks")]
        public async Task<IActionResult> GetDataLocks(long apprenticeshipId)
        {
            var result = await _mediator.Send(new GetDataLocksQuery(apprenticeshipId));

            var response = await _modelMapper.Map<GetDataLocksResponse>(result);
            return Ok(response);
        }

        [HttpGet("{apprenticeshipId}/datalocksummaries")]
        public async Task<IActionResult> GetDataLockSummaries(long apprenticeshipId)
        {
            var result = await _mediator.Send(new GetDataLockSummariesQuery(apprenticeshipId));

            var response = await _modelMapper.Map<GetDataLockSummariesResponse>(result);
            return Ok(response);
        }

        [HttpPost]
        [Route("{apprenticeshipId}/datalocks/accept-changes")]
        public async Task<IActionResult> AcceptDataLockChanges(long apprenticeshipId, [FromBody] AcceptDataLocksRequestChangesRequest request)
        {
            await _mediator.Send(new AcceptDataLocksRequestChangesCommand(
                apprenticeshipId,
                request.UserInfo));

            return Ok();
        }

        [HttpPost]
        [Route("{apprenticeshipId}/datalocks/reject-changes")]
        public async Task<IActionResult> RejectDataLockChanges(long apprenticeshipId, [FromBody] RejectDataLocksRequestChangesRequest request)
        {
            await _mediator.Send(new RejectDataLocksRequestChangesCommand(
                apprenticeshipId,
                request.UserInfo));

            return Ok();
        }

        [HttpPost]
        [Route("{apprenticeshipId}/datalocks/triage")]
        public async Task<IActionResult> TriageDataLocks(long apprenticeshipId, [FromBody] TriageDataLocksRequest request)
        {
            await _mediator.Send(new TriageDataLocksCommand(
                apprenticeshipId,
                request.TriageStatus,
                request.UserInfo));

            return Ok();
        }
    }
}