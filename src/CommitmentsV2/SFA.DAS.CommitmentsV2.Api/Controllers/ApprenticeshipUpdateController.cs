using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

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
        [Route("apprenticeshipupdates")]
        public async Task<IActionResult> GetApprenticeshipUpdates(GetApprenticeshipUpdateRequest request)
        {
            var result = await _mediator.Send(new GetApprenticeshipUpdateQuery(request.ApprenticeshipId, request.Status));
            var response = await _modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
            return Ok(response);
        }
    }
}