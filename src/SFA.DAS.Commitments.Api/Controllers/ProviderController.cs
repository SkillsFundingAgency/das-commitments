﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/provider")]
    public class ProviderController : ApiController
    {
        private readonly ProviderOrchestrator _providerOrchestrator;

        private readonly ApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        public ProviderController(ProviderOrchestrator providerOrchestrator, ApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            if (providerOrchestrator == null)
                throw new ArgumentNullException(nameof(providerOrchestrator));
            if (apprenticeshipsOrchestrator == null)
                throw new ArgumentNullException(nameof(apprenticeshipsOrchestrator));

            _providerOrchestrator = providerOrchestrator;
            _apprenticeshipsOrchestrator = apprenticeshipsOrchestrator;
        }

        [Route("{providerId}/commitments")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitments(long providerId)
        {
            var response = await _providerOrchestrator.GetCommitments(providerId);

            var commitments = response;

            return Ok(commitments);
        }

        [Route("{providerId}/commitments/{commitmentId}", Name = "GetCommitmentForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitment(long providerId, long commitmentId)
        {
            var response = await _providerOrchestrator.GetCommitment(providerId, commitmentId);

            var commitment = response;

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

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/search")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long providerId, [FromUri] ApprenticeshipSearchQuery query)
        {
            var response = await _providerOrchestrator.GetApprenticeships(providerId, query);

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeship(long providerId, long apprenticeshipId)
        {
            var response = await _providerOrchestrator.GetApprenticeship(providerId, apprenticeshipId);

            var apprenticeship = response;

            if (apprenticeship == null)
            {
                return NotFound();
            }

            return Ok(apprenticeship);
        }


        [Route("{providerId}/commitments/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchCommitment(long providerId, long commitmentId, [FromBody] CommitmentSubmission submission)
        {
            await _providerOrchestrator.PatchCommitment(providerId, commitmentId, submission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteCommitment(long providerId, long commitmentId, [FromBody] DeleteRequest deleteRequest)
        {
            await _providerOrchestrator.DeleteCommitment(providerId, commitmentId, deleteRequest.UserId, deleteRequest.LastUpdatedByInfo?.Name);

            return StatusCode(HttpStatusCode.NoContent);
        }


        [Route("{providerId}/commitments/{commitmentId}/apprenticeships", Name = "CreateApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateApprenticeship(long providerId, long commitmentId, [FromBody] ApprenticeshipRequest apprenticeshipRequest)
        {
            var response = await _providerOrchestrator.CreateApprenticeship(providerId, commitmentId, apprenticeshipRequest);

            return CreatedAtRoute("GetApprenticeshipForProvider", new {providerId, commitmentId, apprenticeshipId = response}, default(Apprenticeship));
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}", Name = "UpdateApprenticeshipForProvider")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, ApprenticeshipRequest apprenticeshipRequest)
        {
            await _providerOrchestrator.PutApprenticeship(providerId, commitmentId, apprenticeshipId, apprenticeshipRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/commitments/{commitmentId}/apprenticeships/bulk")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PostBulkUpload(long providerId, long commitmentId, BulkApprenticeshipRequest bulkRequest)
        {
            // TODO: What should we return to the caller? list of urls?
            await _providerOrchestrator.CreateApprenticeships(providerId, commitmentId, bulkRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [Route("{providerId}/bulkupload")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> BulkUploadFile(long providerId, [FromBody] BulkUploadFileRequest bulkUploadFile)
        {
            var bulkUploadFileId = await _providerOrchestrator.PostBulkUploadFile(providerId, bulkUploadFile);

            return CreatedAtRoute("GetBulkUploadFile", new { providerId, bulkUploadFileId }, bulkUploadFileId);
        }

        [HttpGet]
        [Route("{providerId}/bulkupload/{bulkUploadFileId}", Name = "GetBulkUploadFile")]
        [Authorize(Roles = "Role1")]
        public async Task<HttpResponseMessage> BulkUploadFile(long providerId, long bulkUploadFileId)
        {
            var file = await _providerOrchestrator.GetBulkUploadFile(providerId, bulkUploadFileId);

            if (file == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(file, Encoding.UTF8, "application/json");
            return response;
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteApprenticeship(long providerId, long apprenticeshipId, [FromBody] DeleteRequest deleteRequest)
        {
            await _providerOrchestrator.DeleteApprenticeship(providerId, apprenticeshipId, deleteRequest.UserId, deleteRequest.LastUpdatedByInfo?.Name);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/relationships/{employerAccountId}/{legalEntityId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetRelationshipByProviderAndLegalEntityId(long providerId, long employerAccountId, string legalEntityId)
        {
            var response = await _providerOrchestrator.GetRelationship(providerId, employerAccountId, legalEntityId);
            return Ok(response);
        }

        [Route("{providerId}/relationships/{commitmentId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetRelationshipByCommitment(long providerId, long commitmentId)
        {
            var response = await _providerOrchestrator.GetRelationship(providerId, commitmentId);
            return Ok(response);
        }

        [Route("{providerId}/relationships/{employerAccountId}/{legalEntityId}")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchRelationship(long providerId, long employerAccountId, string legalEntityId, [FromBody] RelationshipRequest request)
        {
            await _providerOrchestrator.PatchRelationship(providerId, employerAccountId, legalEntityId, request);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/update")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetPendingApprenticeshipUpdate(long providerId, long apprenticeshipId)
        {
            var response = await _providerOrchestrator.GetPendingApprenticeshipUpdate(providerId, apprenticeshipId);
            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/update")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateApprenticeshipUpdate(long providerId, long apprenticeshipId,
            [FromBody] ApprenticeshipUpdateRequest updateRequest)
        {
            await _providerOrchestrator.CreateApprenticeshipUpdate(providerId, updateRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/update")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchApprenticeshipUpdate(long providerId, long apprenticeshipId, [FromBody] ApprenticeshipUpdateSubmission apprenticeshipSubmission)
        {
            await _providerOrchestrator.PatchApprenticeshipUpdate(providerId, apprenticeshipId, apprenticeshipSubmission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/prices")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetPriceHistory(long providerId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetPriceHistory(apprenticeshipId, new Caller(providerId, CallerType.Provider));

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocks")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLocks(long providerId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetDataLocks(apprenticeshipId, new Caller(providerId, CallerType.Provider));

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocksummary")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLockSummary(long providerId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetDataLockSummary(apprenticeshipId, new Caller(providerId, CallerType.Provider));

            return Ok(response);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocks/{dataLockEventId}")]
        [HttpPatch]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchDataLock(long providerId, long apprenticeshipId, long dataLockEventId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _apprenticeshipsOrchestrator.TriageDataLock(apprenticeshipId, dataLockEventId, triageSubmission, new Caller(providerId, CallerType.Provider));
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{providerId}/apprenticeships/{apprenticeshipId}/datalocks")]
        [HttpPatch]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchDataLocks(long providerId, long apprenticeshipId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _apprenticeshipsOrchestrator.TriageDataLocks(apprenticeshipId, triageSubmission, new Caller(providerId, CallerType.Provider));
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
