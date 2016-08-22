using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Controllers
{
    public class CommitmentsController : ApiController
    {
        private IMediator _mediator;

        public CommitmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/ProviderController/5
        public async Task<IHttpActionResult> Get(long id)
        {
            var response = await _mediator.SendAsync(new GetProviderCommitmentsRequest { ProviderId = id });

            if (response.HasError)
            {
                return NotFound();
            }

            return Ok(response.Commitments);
        }
    }
}
