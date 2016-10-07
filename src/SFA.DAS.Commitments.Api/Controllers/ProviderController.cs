using System;
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

        [Route("{id}/commitments")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitments(long id)
        {
            var response = await _providerOrchestrator.GetCommitments(id);

            var commitments = response.Data;

            return Ok(commitments);
        }

        [Route("{providerId}/commitments/{commitmentId}")]
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

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            var response = await _providerOrchestrator.GetApprenticeship(providerId, commitmentId, apprenticeshipId);

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

            return CreatedAtRoute("GetApprenticeshipForProvider", new { providerId = providerId, commitmentId = commitmentId, apprenticeshipId = response }, default(Apprenticeship));
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
        public async Task<IHttpActionResult> PatchCommitment(long providerId, long commitmentId, [FromBody]CommitmentStatus? status)
        {
            await _providerOrchestrator.PatchCommitment(providerId, commitmentId, status);

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
