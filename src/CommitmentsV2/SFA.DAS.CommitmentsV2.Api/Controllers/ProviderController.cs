using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;

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
        public async Task<IActionResult> GetAllProviders()
        {
            var result = await _mediator.Send(new GetAllProvidersQuery());

            return Ok(new GetAllProvidersResponse
            {
                Providers = result.Providers
            });
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

        [HttpGet]
        [Route("{providerId}/commitmentagreements")]
        public async Task<IActionResult> GetCommitmentAgreements(long providerId)
        {
            var query = new GetProviderCommitmentAgreementQuery(providerId);
            var result = await _mediator.Send(query);
            
            return Ok(new GetProviderCommitmentAgreementResponse
            {
              ProviderCommitmentAgreement = result.Agreements
            });
        }
    }
}