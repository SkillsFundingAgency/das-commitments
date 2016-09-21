using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
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

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        public async Task<IHttpActionResult> GetApprenticeship(long accountId, long commitmentId, long apprenticeshipId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetApprenticeshipRequest { AccountId = accountId, CommitmentId = commitmentId, ApprenticeshipId = apprenticeshipId });

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

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        public async Task<IHttpActionResult> PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipCommand { AccountId = accountId, CommitmentId = commitmentId, ApprenticeshipId = apprenticeshipId, Apprenticeship = apprenticeship });

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }

        [Route("{accountId}/commitments/{commitmentId}")]

        public async Task<IHttpActionResult> PatchCommitment(long accountId, long commitmentId, [FromBody]CommitmentStatus? status)
        {
            try
            {
                await _mediator.SendAsync(new UpdateCommitmentStatusCommand
                {
                    AccountId = accountId,
                    CommitmentId = commitmentId,
                    Status = status
                });

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]

        public async Task<IHttpActionResult> PatchApprenticeship(long accountId, long commitmentId, long apprenticeshipId, [FromBody]ApprenticeshipStatus? status)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipStatusCommand { AccountId = accountId, CommitmentId = commitmentId, ApprenticeshipId = apprenticeshipId, Status = status });

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (InvalidRequestException)
            {
                return BadRequest();
            }
        }
    }
}
