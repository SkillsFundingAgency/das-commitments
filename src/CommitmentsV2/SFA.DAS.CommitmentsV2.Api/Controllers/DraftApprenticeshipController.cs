using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping;

using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;
using GetDraftApprenticeshipCommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice.GetDraftApprenticeResponse;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts/{cohortId}/draft-apprenticeships")]
    [ApiController]
    [Authorize]
    public class DraftApprenticeshipController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> _updateDraftApprenticeshipMapper;
        private readonly IMapper<GetDraftApprenticeshipCommandResponse, GetDraftApprenticeshipResponse> _getDraftApprenticeshipMapper;
        private readonly IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> _addDraftApprenticeshipMapper;
        private readonly IMapper<GetDraftApprenticeshipsResult, GetDraftApprenticeshipsResponse> _getDraftApprenticeshipsResultMapper;

        public DraftApprenticeshipController(
            IMediator mediator,
            IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> updateDraftApprenticeshipMapper,
            IMapper<GetDraftApprenticeshipCommandResponse, GetDraftApprenticeshipResponse> getDraftApprenticeshipMapper,
            IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> addDraftApprenticeshipMapper, IMapper<GetDraftApprenticeshipsResult, GetDraftApprenticeshipsResponse> getDraftApprenticeshipsResultMapper)
        {
            _mediator = mediator;
            _updateDraftApprenticeshipMapper = updateDraftApprenticeshipMapper;
            _getDraftApprenticeshipMapper = getDraftApprenticeshipMapper;
            _addDraftApprenticeshipMapper = addDraftApprenticeshipMapper;
            _getDraftApprenticeshipsResultMapper = getDraftApprenticeshipsResultMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(long cohortId)
        {
            var response = await _mediator.Send(new GetDraftApprenticeshipsRequest(cohortId));
            var result = await _getDraftApprenticeshipsResultMapper.Map(response);

            if (result.DraftApprenticeships == null)
            {
                return NotFound();
            }
            return Ok(result.DraftApprenticeships);
        }

        [HttpGet]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Get(long cohortId, long apprenticeshipId)
        {
            var command = new GetDraftApprenticeRequest(cohortId, apprenticeshipId);

            var response = await _mediator.Send(command);

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
    }
}
