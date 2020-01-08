using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [Route("pending")]
        public async Task<IActionResult> GetPendingUpdate(long apprenticeshipId)
        {
            var result = await _mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId));

            var response = await _modelMapper.Map<GetApprenticeshipUpdateResponse>(result);
            return Ok(response);
        }
    }
}