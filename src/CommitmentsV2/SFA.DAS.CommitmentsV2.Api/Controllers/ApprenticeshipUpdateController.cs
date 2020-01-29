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
    [Route("api/apprenticeships/{ApprenticeshipId}")]
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
        public async Task<IActionResult> GetApprenticeshipUpdates(long apprenticeshipId,[FromQuery] ApprenticeshipUpdateStatus? status)
        {
            var result = await _mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId, status));
            var response = await _modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
            return Ok(response);
        }
    }
}