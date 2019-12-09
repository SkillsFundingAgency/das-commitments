using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Route("api/providers")]
    public class ProviderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProviderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("approved")]
        public async Task<IActionResult> GetApprovedProviders(GetApprovedProvidersRequest request)
        {
            var query = new GetApprovedProvidersQuery(request.AccountId);

            var result = await  _mediator.Send(query);

            return Ok(new GetApprovedProvidersResponse(result.ProviderIds));
        }
        
        [HttpGet]
        [Route("{providerId}")]
        public async Task<IActionResult> GetProvider(long providerId)
        {
            var query = new GetProviderQuery(providerId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound();
            }
            
            return Ok(new GetProviderResponse
            {
                ProviderId = result.ProviderId,
                Name = result.Name
            });
        }
    }
}