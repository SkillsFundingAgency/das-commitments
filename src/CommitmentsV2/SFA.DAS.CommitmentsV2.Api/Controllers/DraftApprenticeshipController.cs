using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping;
using UpdateDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.UpdateDraftApprenticeshipResponse;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    public class DraftApprenticeshipController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> _updateDraftApprenticeshipMapper;

        public DraftApprenticeshipController(
            IMediator mediator,
            IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> updateDraftApprenticeshipMapper)
        {
            _mediator = mediator;
            _updateDraftApprenticeshipMapper = updateDraftApprenticeshipMapper;
        }

        [HttpPatch]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Update(long apprenticeshipId, [FromBody]UpdateDraftApprenticeshipRequest request)
        {
            var command = _updateDraftApprenticeshipMapper.Map(request);
            command.ApprenticeshipId = apprenticeshipId;

            await _mediator.Send(command);

            return Ok(new UpdateDraftApprenticeshipResponse());
        }
    }
}