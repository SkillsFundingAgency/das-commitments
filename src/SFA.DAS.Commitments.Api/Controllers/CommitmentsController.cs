using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/commitments")]
    public class CommitmentsController : ApiController
    {
        private readonly IMediator _mediator;

        public CommitmentsController(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<IHttpActionResult> GetAll(long id)
        {
            QueryResponse<IList<CommitmentListItem>> response;

            if (id % 2 == 1)
            {
                response = await _mediator.SendAsync(new GetProviderCommitmentsRequest { ProviderId = id });
            }
            else
            {
                response = await _mediator.SendAsync(new GetEmployerCommitmentsRequest { AccountId = id });
            }

            if (response.HasErrors)
            {
                return BadRequest();
            }

            return Ok(response.Data);
        }

        [Route("{id}")]
        public async Task<IHttpActionResult> Get(long id)
        {
            var response = await _mediator.SendAsync(new GetCommitmentRequest { CommitmentId = id });

            if (response.HasErrors)
            {
                return BadRequest();
            }

            if (response.Data == null)
            {
                return NotFound();
            }

            return Ok(response.Data);
        }
    }
}
