using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{apprenticeshipId}/price-episodes")]
    public class PriceEpisodesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceEpisodesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task Get(long apprenticeshipId)
        {
            var query = new GetPriceEpisodesQuery(apprenticeshipId);
            var result = await _mediator.Send(query);



        }
    }
}
