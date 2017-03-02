using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/employer")]
    public class EmployerController : ApiController
    {
        private readonly EmployerOrchestrator _employerOrchestrator;

        public EmployerController(EmployerOrchestrator employerOrchestrator)
        {
            if (employerOrchestrator == null)
                throw new ArgumentNullException(nameof(employerOrchestrator));
            _employerOrchestrator = employerOrchestrator;
        }

        [Route("{accountId}/commitments")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitments(long accountId)
        {
            var response = await _employerOrchestrator.GetCommitments(accountId);

            return Ok(response.Data);
        }

        [Route("{accountId}/commitments/{commitmentId}", Name = "GetCommitmentForEmployer")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitment(long accountId, long commitmentId)
        {
            var response = await _employerOrchestrator.GetCommitment(accountId, commitmentId);

            if (response.Data == null)
            {
                return NotFound();
            }

            return Ok(response.Data);
        }

        [Route("{accountId}/apprenticeships")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long accountId)
        {
            var response = await _employerOrchestrator.GetApprenticeships(accountId);

            return Ok(response.Data);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForEmployer")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeship(long accountId, long apprenticeshipId)
        {
            var response = await _employerOrchestrator.GetApprenticeship(accountId, apprenticeshipId);

            if (response.Data == null)
            {
                return NotFound();
            }

            return Ok(response.Data);
        }


        [Route("{accountId}/commitments/")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateCommitment(long accountId, CommitmentRequest commitment)
        {
            var response = await _employerOrchestrator.CreateCommitment(accountId, commitment);

            return CreatedAtRoute("GetCommitmentForEmployer", new { accountId, commitmentId = response }, new Commitment { Id = response });
        }

        [Route("{accountId}/commitments/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchCommitment(long accountId, long commitmentId, [FromBody] CommitmentSubmission values)
        {
            await _employerOrchestrator.PatchCommitment(accountId, commitmentId, values);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/commitments/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteCommitment(long accountId, long commitmentId, [FromBody] DeleteRequest deleteRequest)
        {
            await _employerOrchestrator.DeleteCommitment(accountId, commitmentId, deleteRequest.UserId);

            return StatusCode(HttpStatusCode.NoContent);
        }


        [Route("{accountId}/commitments/{commitmentId}/apprenticeships", Name = "CreateApprenticeshipForEmployer")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateApprenticeship(long accountId, long commitmentId, ApprenticeshipRequest apprenticeshipRequest)
        {
            var response = await _employerOrchestrator.CreateApprenticeship(accountId, commitmentId, apprenticeshipRequest);

            return CreatedAtRoute("GetApprenticeshipForEmployer", new { accountId, commitmentId, apprenticeshipId = response }, default(Apprenticeship));
        }


        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, ApprenticeshipRequest apprenticeshipRequest)
        {
            await _employerOrchestrator.PutApprenticeship(accountId, commitmentId, apprenticeshipId, apprenticeshipRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchApprenticeship(long accountId, long commitmentId, long apprenticeshipId, [FromBody] ApprenticeshipSubmission apprenticeshipSubmission)
        {
            await _employerOrchestrator.PatchApprenticeship(accountId, commitmentId, apprenticeshipId, apprenticeshipSubmission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteApprenticeship(long accountId, long apprenticeshipId, [FromBody] DeleteRequest deleteRequest)
        {
            await _employerOrchestrator.DeleteApprenticeship(accountId, apprenticeshipId, deleteRequest.UserId);

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
