using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{apprenticeshipId}/price-episodes")]
    public class PriceEpisodeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public PriceEpisodeController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(long apprenticeshipId)
        {
            var query = new GetPriceEpisodesQuery(apprenticeshipId);
            var result = await _mediator.Send(query);

            var response = await _modelMapper.Map<GetPriceEpisodesResponse>(result);
            return Ok(response);
        }
    }
}
