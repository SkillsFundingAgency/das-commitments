using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Orchestrators;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly EmployerOrchestrator _employerOrchestrator;
        private readonly IMediator _mediator;

        public EmployerController(EmployerOrchestrator employerOrchestrator, IMediator mediator)
        {
            if (employerOrchestrator == null)
                throw new ArgumentNullException(nameof(employerOrchestrator));
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _employerOrchestrator = employerOrchestrator;
            _mediator = mediator;
        }

        [Route("{id}/commitments")]
        public async Task<IHttpActionResult> GetCommitments(long id)
        {
            var response = await _employerOrchestrator.GetCommitments(id);

            return Ok(response.Data);
        }

        [Route("{accountId}/commitments/{commitmentId}", Name = "GetCommitmentForEmployer")]
        public async Task<IHttpActionResult> GetCommitment(long accountId, long commitmentId)
        {
            var response = await _employerOrchestrator.GetCommitment(accountId, commitmentId);

            if (response.Data == null)
            {
                return NotFound();
            }

            return Ok(response.Data);
        }

        [Route("{accountId}/commitments/")]
        public async Task<IHttpActionResult> CreateCommitment(long accountId, Commitment commitment)
        {
            var response = await _employerOrchestrator.CreateCommitment(accountId, commitment);

            return CreatedAtRoute("GetCommitmentForEmployer", new { accountId = accountId, commitmentId = response.Data }, default(Commitment));
        }

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        public async Task<IHttpActionResult> GetApprenticeship(long accountId, long commitmentId, long apprenticeshipId)
        {
            var response = await _employerOrchestrator.GetApprenticeship(accountId, commitmentId, apprenticeshipId);

            if (response.Data == null)
            {
                return NotFound();
            }

            return Ok(response.Data);
        }

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        public async Task<IHttpActionResult> PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            await _employerOrchestrator.PutApprenticeship(accountId, commitmentId, apprenticeshipId, apprenticeship);

            return StatusCode(HttpStatusCode.NoContent);
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
