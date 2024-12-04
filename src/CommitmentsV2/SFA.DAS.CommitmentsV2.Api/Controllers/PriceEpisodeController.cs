using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/apprenticeships/{apprenticeshipId:long}/price-episodes")]
public class PriceEpisodeController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(long apprenticeshipId)
    {
        var query = new GetPriceEpisodesQuery(apprenticeshipId);
        var result = await mediator.Send(query);

        var response = await modelMapper.Map<GetPriceEpisodesResponse>(result);
        return Ok(response);
    }
}