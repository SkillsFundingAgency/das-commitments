using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{ApprenticeshipId}/updates")]
    public class ApprenticeshipUpdateController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public ApprenticeshipUpdateController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetApprenticeshipUpdates(long apprenticeshipId,[FromQuery] GetApprenticeshipUpdatesRequest request)
        {
            var result = await _mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId, request.Status));
            var response = await _modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptApprenticeshipUpdates(long apprenticeshipId, [FromQuery] AcceptApprenticeshipUpdatesRequest request)
        {
            var result = await _mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId, request.Status));
            var response = await _modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
            return Ok(response);
        }
    }
}