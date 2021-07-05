using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping;

using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts/{cohortId}/draft-apprenticeships")]
    [ApiController]
    [Authorize]
    public class DraftApprenticeshipController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> _updateDraftApprenticeshipMapper;
        private readonly IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse> _getDraftApprenticeshipMapper;
        private readonly IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> _addDraftApprenticeshipMapper;
        private readonly IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse> _getDraftApprenticeshipsResultMapper;
        private readonly IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand> _deleteDraftApprenticeshipsMapper;

        public DraftApprenticeshipController(
            IMediator mediator,
            IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> updateDraftApprenticeshipMapper,
            IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse> getDraftApprenticeshipMapper,
            IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> addDraftApprenticeshipMapper, 
            IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse> getDraftApprenticeshipsResultMapper,
            IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand> deleteDraftApprenticeshipsMapper
            )
        {
            _mediator = mediator;
            _updateDraftApprenticeshipMapper = updateDraftApprenticeshipMapper;
            _getDraftApprenticeshipMapper = getDraftApprenticeshipMapper;
            _addDraftApprenticeshipMapper = addDraftApprenticeshipMapper;
            _getDraftApprenticeshipsResultMapper = getDraftApprenticeshipsResultMapper;
            _deleteDraftApprenticeshipsMapper = deleteDraftApprenticeshipsMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(long cohortId)
        {
            var result = await _mediator.Send(new GetDraftApprenticeshipsQuery(cohortId));
            var response = await _getDraftApprenticeshipsResultMapper.Map(result);

            if (response.DraftApprenticeships == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Get(long cohortId, long apprenticeshipId)
        {
            var command = new GetDraftApprenticeshipQuery(cohortId, apprenticeshipId);

            var response = await _mediator.Send(command);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(await _getDraftApprenticeshipMapper.Map(response));
        }

        [HttpPut]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody]UpdateDraftApprenticeshipRequest request)
        {
            var command = await _updateDraftApprenticeshipMapper.Map(request);
            command.CohortId = cohortId;
            command.ApprenticeshipId = apprenticeshipId;

            await _mediator.Send(command);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Add(long cohortId, [FromBody]AddDraftApprenticeshipRequest request)
        {
            var command = await _addDraftApprenticeshipMapper.Map(request);

            command.CohortId = cohortId;
            
            var result = await _mediator.Send(command);
            
            return Ok(new AddDraftApprenticeshipResponse
            {
                DraftApprenticeshipId = result.Id
            });
        }

        [HttpPost]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Delete(long cohortId, long apprenticeshipId, [FromBody]DeleteDraftApprenticeshipRequest request)
        {
            var command = await _deleteDraftApprenticeshipsMapper.Map(request);
            command.CohortId = cohortId;
            command.ApprenticeshipId = apprenticeshipId;

            await _mediator.Send(command);

            return Ok();
        }
    }
}