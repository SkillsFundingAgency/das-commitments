using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts/{cohortId}/draft-apprenticeships")]
    public class DraftApprenticeshipController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> _addDraftApprenticeshipMapper;

        public DraftApprenticeshipController(
            IMediator mediator,
            IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> addDraftApprenticeshipMapper)
        {
            _mediator = mediator;
            _addDraftApprenticeshipMapper = addDraftApprenticeshipMapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddDraftApprenticeship(long cohortId, [FromBody]AddDraftApprenticeshipRequest request)
        {
            var command = _addDraftApprenticeshipMapper.Map(request);

            command.CohortId = cohortId;
            
            var result = await _mediator.Send(command);
            
            return Ok(new AddDraftApprenticeshipResponse
            {
                DraftApprenticeshipId = result.Id
            });
        }
    }
}