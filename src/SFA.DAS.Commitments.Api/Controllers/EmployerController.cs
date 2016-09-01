using System;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/employer")]
    public class EmployerController : ApiController
    {
        private readonly IMediator _mediator;

        public EmployerController(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        [Route("{id}/commitments")]
        public async Task<IHttpActionResult> GetCommitments(long id)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetEmployerCommitmentsRequest { AccountId = id });

                return Ok(response.Data);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }

        [Route("{accountId}/commitments/{commitmentId}", Name = "GetCommitmentForEmployer")]
        public async Task<IHttpActionResult> GetCommitment(long accountId, long commitmentId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetCommitmentRequest { AccountId = accountId, CommitmentId = commitmentId });

                if (response.Data == null)
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

        [Route("{accountId}/commitments/")]
        public async Task<IHttpActionResult> CreateCommitment(long accountId, Commitment commitment)
        {
            try
            {
                var commitmentId = await _mediator.SendAsync(new CreateCommitmentCommand { Commitment = commitment });

                return CreatedAtRoute("GetCommitmentForEmployer", new { accountId = accountId, commitmentId = commitmentId }, default(Commitment));
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}
