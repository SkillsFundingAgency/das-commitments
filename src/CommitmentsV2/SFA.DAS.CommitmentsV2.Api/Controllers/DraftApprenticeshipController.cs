using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice;
using SFA.DAS.CommitmentsV2.Mapping;

using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;
using GetDraftApprenticeshipCommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice.GetDraftApprenticeResponse;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts/{cohortId}/draft-apprenticeships")]
    public class DraftApprenticeshipController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> _updateDraftApprenticeshipMapper;

        private readonly IMapper<GetDraftApprenticeshipCommandResponse, GetDraftApprenticeshipResponse> _getDraftApprenticeshipMapper;

        public DraftApprenticeshipController(
            IMediator mediator,
            IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> updateDraftApprenticeshipMapper,
            IMapper<GetDraftApprenticeshipCommandResponse, GetDraftApprenticeshipResponse> getDraftApprenticeshipMapper)
        {
            _mediator = mediator;
            _updateDraftApprenticeshipMapper = updateDraftApprenticeshipMapper;
            _getDraftApprenticeshipMapper = getDraftApprenticeshipMapper;
        }

        [HttpGet]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Get(long cohortId, long apprenticeshipId)
        {
            var command = new GetDraftApprenticeRequest(cohortId, apprenticeshipId);

            var response = await _mediator.Send(command);

            return Ok(_getDraftApprenticeshipMapper.Map(response));
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
    }
}