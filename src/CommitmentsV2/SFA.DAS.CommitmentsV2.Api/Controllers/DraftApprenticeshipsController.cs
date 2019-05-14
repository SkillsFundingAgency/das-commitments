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
    public class DraftApprenticeshipsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> _addDraftApprenticeshipMapper;

        public DraftApprenticeshipsController(IMediator mediator, IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> addDraftApprenticeshipMapper)
        {
            _mediator = mediator;
            _addDraftApprenticeshipMapper = addDraftApprenticeshipMapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddDraftApprenticeship([FromBody]AddDraftApprenticeshipRequest request)
        {
            await _mediator.Send(_addDraftApprenticeshipMapper.Map(request));
            
            return Ok(new AddDraftApprenticeshipResponse());
        }
    }
}