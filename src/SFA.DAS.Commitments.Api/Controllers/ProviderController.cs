using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/provider")]
    public class ProviderController : ApiController
    {
        private readonly ProviderOrchestrator _providerOrchestrator;

        public ProviderController(ProviderOrchestrator providerOrchestrator)
        {
            if (providerOrchestrator == null)
                throw new ArgumentNullException(nameof(providerOrchestrator));
            _providerOrchestrator = providerOrchestrator;
        }

        [Route("{providerId}/commitments")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitments(long providerId)
        {
            var response = await _providerOrchestrator.GetCommitments(providerId);

            var commitments = response.Data;

            return Ok(commitments);
        }

        [Route("{providerId}/commitments/{commitmentId}", Name = "GetCommitmentForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitment(long providerId, long commitmentId)
        {
            var response = await _providerOrchestrator.GetCommitment(providerId, commitmentId);

            var commitment = response.Data;

            if (commitment == null)
            {
                return NotFound();
            }

            return Ok(commitment);
        }

        [Route("{providerId}/apprenticeships")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long providerId)
        {
            var response = await _providerOrchestrator.GetApprenticeships(providerId);

            return Ok(response.Data);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeship(long providerId, long apprenticeshipId)
        {
            var response = await _providerOrchestrator.GetApprenticeship(providerId, apprenticeshipId);

            var apprenticeship = response.Data;

            if (apprenticeship == null)
            {
                return NotFound();
            }

            return Ok(apprenticeship);
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships", Name = "CreateApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateApprenticeship(long providerId, long commitmentId, Apprenticeship apprenticeship)
        {
            var response = await _providerOrchestrator.CreateApprenticeship(providerId, commitmentId, apprenticeship);

            return CreatedAtRoute("GetApprenticeshipForProvider", new {providerId, commitmentId, apprenticeshipId = response}, default(Apprenticeship));
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}", Name = "UpdateApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            await _providerOrchestrator.PutApprenticeship(providerId, commitmentId, apprenticeshipId, apprenticeship);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchCommitment(long providerId, long commitmentId, [FromBody] CommitmentSubmission submission)
        {
            await _providerOrchestrator.PatchCommitment(providerId, commitmentId, submission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/bulk")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PostBulkUpload(long providerId, long commitmentId, IList<Apprenticeship> apprenticeships)
        {
            // TODO: What should we return to the caller? list of urls?
            await _providerOrchestrator.CreateApprenticeships(providerId, commitmentId, apprenticeships);

            return CreatedAtRoute("GetCommitmentForProvider", new { providerId, commitmentId = commitmentId }, default(Commitment));
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteApprenticeship(long providerId, long apprenticeshipId)
        {
            await _providerOrchestrator.DeleteApprenticeship(providerId, apprenticeshipId);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteCommitment(long providerId, long commitmentId)
        {
            await _providerOrchestrator.DeleteCommitment(providerId, commitmentId);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/relationships/{employerAccountId}/{legalEntityId}")]
        //[Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetRelationshipByProviderAndLegalEntityId(long providerId, long employerAccountId, string legalEntityId)
        {
            var response = await _providerOrchestrator.GetRelationship(providerId, employerAccountId, legalEntityId);
            return Ok(response.Data);
        }

        [Route("{providerId}/relationships/{employerAccountId}/{legalEntityId}")]
        [HttpPatch]
        //[Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchRelationship(long providerId, long employerAccountId, string legalEntityId, [FromBody] RelationshipRequest request)
        {
            await _providerOrchestrator.PatchRelationship(providerId, employerAccountId, legalEntityId, request);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
