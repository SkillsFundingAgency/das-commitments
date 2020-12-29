using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Infrastructure.Authorization;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/provider")]
    public class ProviderController : ApiController
    {
        private readonly IProviderOrchestrator _providerOrchestrator;
        private readonly IApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        public ProviderController(IProviderOrchestrator providerOrchestrator, IApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            _providerOrchestrator = providerOrchestrator;
            _apprenticeshipsOrchestrator = apprenticeshipsOrchestrator;
        }

        [Route("{providerId")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        [HttpGet]
        public async Task<IHttpActionResult> GetProvider(long providerId)
        {
            var response = await _providerOrchestrator.GetProvider(providerId);
            return Ok(response);
        }
        

        [Route("{providerId}/commitments")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitments(long providerId)
        {
            var response = await _providerOrchestrator.GetCommitments(providerId);

            return Ok(response);
        }


        [Route("{providerId}/commitments/")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public IHttpActionResult CreateCommitment(long providerId, CommitmentRequest commitment)
        {
            throw new InvalidOperationException();
        }

        [Route("{providerId}/commitmentagreements")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitmentAgreements(long providerId)
        {
            return Ok(await _providerOrchestrator.GetCommitmentAgreements(providerId));
        }

        [Route("{providerId}/commitments/{commitmentId}", Name = "GetCommitmentForProvider")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitment(long providerId, long commitmentId)
        {
            var response = await _providerOrchestrator.GetCommitment(providerId, commitmentId);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long providerId)
        {
            var response = await _providerOrchestrator.GetApprenticeships(providerId);

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/search")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long providerId, [FromUri] ApprenticeshipSearchQuery query)
        {
            var response = await _providerOrchestrator.GetApprenticeships(providerId, query);

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForProvider")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeship(long providerId, long apprenticeshipId)
        {
            var response = await _providerOrchestrator.GetApprenticeship(providerId, apprenticeshipId);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }


        [Route("{providerId}/commitments/{commitmentId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchCommitment(long providerId, long commitmentId, [FromBody] CommitmentSubmission submission)
        {
            await _providerOrchestrator.PatchCommitment(providerId, commitmentId, submission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}/approve")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        [HttpPatch]
        public async Task<IHttpActionResult> ApproveCohort(long providerId, long commitmentId, [FromBody] CommitmentSubmission submission)
        {
            await _providerOrchestrator.ApproveCohort(providerId, commitmentId, submission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteCommitment(long providerId, long commitmentId, [FromBody] DeleteRequest deleteRequest)
        {
            await _providerOrchestrator.DeleteCommitment(providerId, commitmentId, deleteRequest.UserId, deleteRequest.LastUpdatedByInfo?.Name);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/bulk")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PostBulkUpload(long providerId, long commitmentId, BulkApprenticeshipRequest bulkRequest)
        {
            // TODO: What should we return to the caller? list of urls?
            await _providerOrchestrator.CreateApprenticeships(providerId, commitmentId, bulkRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("{providerId}/bulkupload")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> BulkUploadFile(long providerId, [FromBody] BulkUploadFileRequest bulkUploadFile)
        {
            var bulkUploadFileId = await _providerOrchestrator.PostBulkUploadFile(providerId, bulkUploadFile);

            return CreatedAtRoute("GetBulkUploadFile", new { providerId, bulkUploadFileId }, bulkUploadFileId);
        }

        [HttpGet]
        [Route("{providerId}/bulkupload/{bulkUploadFileId}", Name = "GetBulkUploadFile")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<HttpResponseMessage> BulkUploadFile(long providerId, long bulkUploadFileId)
        {
            var file = await _providerOrchestrator.GetBulkUploadFile(providerId, bulkUploadFileId);

            if (file == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(file, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteApprenticeship(long providerId, long apprenticeshipId, [FromBody] DeleteRequest deleteRequest)
        {
            await _providerOrchestrator.DeleteApprenticeship(providerId, apprenticeshipId, deleteRequest.UserId, deleteRequest.LastUpdatedByInfo?.Name);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/update")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetPendingApprenticeshipUpdate(long providerId, long apprenticeshipId)
        {
            var response = await _providerOrchestrator.GetPendingApprenticeshipUpdate(providerId, apprenticeshipId);
            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/update")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateApprenticeshipUpdate(long providerId, long apprenticeshipId,
            [FromBody] ApprenticeshipUpdateRequest updateRequest)
        {
            await _providerOrchestrator.CreateApprenticeshipUpdate(providerId, updateRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/update")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchApprenticeshipUpdate(long providerId, long apprenticeshipId, [FromBody] ApprenticeshipUpdateSubmission apprenticeshipSubmission)
        {
            await _providerOrchestrator.PatchApprenticeshipUpdate(providerId, apprenticeshipId, apprenticeshipSubmission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/prices")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetPriceHistory(long providerId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetPriceHistory(apprenticeshipId, new Caller(providerId, CallerType.Provider));

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocks")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLocks(long providerId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetDataLocks(apprenticeshipId, new Caller(providerId, CallerType.Provider));

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocksummary")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLockSummary(long providerId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetDataLockSummary(apprenticeshipId, new Caller(providerId, CallerType.Provider));

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocks/{dataLockEventId}")]
        [HttpPatch]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchDataLock(long providerId, long apprenticeshipId, long dataLockEventId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _apprenticeshipsOrchestrator.TriageDataLock(apprenticeshipId, dataLockEventId, triageSubmission, new Caller(providerId, CallerType.Provider));
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocks")]
        [HttpPatch]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchDataLocks(long providerId, long apprenticeshipId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _apprenticeshipsOrchestrator.TriageDataLocks(apprenticeshipId, triageSubmission, new Caller(providerId, CallerType.Provider));
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
