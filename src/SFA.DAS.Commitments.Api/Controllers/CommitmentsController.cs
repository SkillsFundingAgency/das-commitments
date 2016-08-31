using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;
using SFA.DAS.Commitments.Application.Queries.GetProviderCommitments;

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

            try
            {
                if (id % 2 == 1)
                {
                    response = await _mediator.SendAsync(new GetProviderCommitmentsRequest { ProviderId = id });
                }
                else
                {
                    response = await _mediator.SendAsync(new GetEmployerCommitmentsRequest { AccountId = id });
                }

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }

        [Route("{id}")]
        public async Task<IHttpActionResult> Get(long id, long? providerId = null, long? accountId = null)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetCommitmentRequest { CommitmentId = id, ProviderId = providerId, AccountId = accountId });

                if (response?.Data == null)
                {
                    return NotFound();
                }

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }
    }
}
