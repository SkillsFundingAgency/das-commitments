using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Application.Queries;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
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

        // GET: api/commitments/5
        public async Task<IHttpActionResult> Get(long id)
        {
            QueryResponse<IList<Commitment>> response;

            if (id % 2 == 0)
            {
                response = await _mediator.SendAsync(new GetProviderCommitmentsRequest { ProviderId = id });
            }
            else
            {
                response = await _mediator.SendAsync(new GetEmployerCommitmentsRequest { AccountId = id });
            }

            if (response.HasError)
            {
                return BadRequest();
            }

            return Ok(response.Data);
        }
    }
}
