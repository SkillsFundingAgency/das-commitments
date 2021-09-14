using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Infrastructure.Authorization;

namespace SFA.DAS.Commitments.Api.Controllers
{
    // apparantly .net 4.7.1 *always* uses the framework system.net.http and ignores any package system.net.http
    // https://github.com/dotnet/corefx/issues/25050
    // but cloud services doesn't support any version > 4.6.2 out of the box, so we'd have to install 4.7.1 on each server
    // https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-dotnet-install-dotnet
    // what a palaver!
    // better to wait until we switch to app services
    // https://azure.microsoft.com/en-us/updates/net-framework-4-7-2-update-coming-to-azure-app-service-apps/
    // or .net core

    [RoutePrefix("api/employer")]
    public class EmployerController : ApiController
    {
        private readonly IEmployerOrchestrator _employerOrchestrator;
        private readonly IApprenticeshipsOrchestrator _apprenticeshipsOrchestrator;

        public EmployerController(IEmployerOrchestrator employerOrchestrator, IApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            _employerOrchestrator = employerOrchestrator;
            _apprenticeshipsOrchestrator = apprenticeshipsOrchestrator;
        }

        [Route("ids", Name = "GetAllEmployerAccountIds")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetAllEmployerAccountIds()
        {
            var response = await _employerOrchestrator.GetEmployerAccountIds();

            return Ok(response);
        }

        [Route("{accountId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetAccountSummary(long accountId)
        {
            var response = await _employerOrchestrator.GetAccountSummary(accountId);

            return Ok(response);
        }

        [Route("{accountId}/commitments")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitments(long accountId)
        {
            var response = await _employerOrchestrator.GetCommitments(accountId);

            return Ok(response);
        }

        [Route("{accountId}/commitments/{commitmentId}", Name = "GetCommitmentForEmployer")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitment(long accountId, long commitmentId)
        {
            var response = await _employerOrchestrator.GetCommitment(accountId, commitmentId);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        
        [Route("{accountId}/apprenticeships")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long accountId)
        {
            var response = await _employerOrchestrator.GetApprenticeships(accountId);

            return Ok(response);
        }

        [Route("{accountId}/apprenticeships/search")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeships(long accountId, [FromUri] ApprenticeshipSearchQuery query)
        {
            var response = await _employerOrchestrator.GetApprenticeships(accountId, query);

            return Ok(response);
        }

        [Route("{accountId}/apprenticeships/uln/{uln}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        [ResponseType(typeof(IEnumerable<Apprenticeship>))]
        public async Task<IHttpActionResult> GetActiveApprenticeshipsForUln(long accountId, string uln)
        {
            var response = await _employerOrchestrator.GetActiveApprenticeshipsForUln(accountId, uln);
            return Ok(response);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}", Name = "GetApprenticeshipForEmployer")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetApprenticeship(long accountId, long apprenticeshipId)
        {
            var response = await _employerOrchestrator.GetApprenticeship(accountId, apprenticeshipId);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [Route("{accountId}/commitments/{commitmentId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchCommitment(long accountId, long commitmentId, [FromBody] CommitmentSubmission values)
        {
            await _employerOrchestrator.PatchCommitment(accountId, commitmentId, values);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/commitments/{commitmentId}/approve")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        [HttpPatch]
        public IHttpActionResult ApproveCohort(long accountId, long commitmentId, [FromBody] CommitmentSubmission values)
        {
            throw new InvalidOperationException();
        }

        [Route("{accountId}/commitments/{commitmentId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteCommitment(long accountId, long commitmentId, [FromBody] DeleteRequest deleteRequest)
        {
            await _employerOrchestrator.DeleteCommitment(accountId, commitmentId, deleteRequest.UserId, deleteRequest.LastUpdatedByInfo?.Name);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/commitments/{commitmentId}/apprenticeships/{apprenticeshipId}/stopdate")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PutApprenticeshipStopDate(long accountId, long commitmentId, long apprenticeshipId, [FromBody]ApprenticeshipStopDate stopDate)
        {
            await _employerOrchestrator.PutApprenticeshipStopDate(accountId, commitmentId, apprenticeshipId, stopDate);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchApprenticeship(long accountId, long apprenticeshipId, [FromBody] ApprenticeshipSubmission apprenticeshipSubmission)
        {
            await _employerOrchestrator.PatchApprenticeship(accountId, apprenticeshipId, apprenticeshipSubmission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> DeleteApprenticeship(long accountId, long apprenticeshipId, [FromBody] DeleteRequest deleteRequest)
        {
            await _employerOrchestrator.DeleteApprenticeship(accountId, apprenticeshipId, deleteRequest.UserId, deleteRequest.LastUpdatedByInfo?.Name);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}/update")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetPendingApprenticeshipUpdate(long accountId, long apprenticeshipId)
        {
            var response = await _employerOrchestrator.GetPendingApprenticeshipUpdate(accountId, apprenticeshipId);
            return Ok(response);
        }


        [Route("{accountId}/apprenticeships/{apprenticeshipId}/update")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> CreateApprenticeshipUpdate(long accountId, long apprenticeshipId,
            [FromBody] ApprenticeshipUpdateRequest updateRequest)
        {
            await _employerOrchestrator.CreateApprenticeshipUpdate(accountId, updateRequest);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}/update")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchApprenticeshipUpdate(long accountId, long apprenticeshipId, [FromBody] ApprenticeshipUpdateSubmission apprenticeshipSubmission)
        {
            await _employerOrchestrator.PatchApprenticeshipUpdate(accountId, apprenticeshipId, apprenticeshipSubmission);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}/prices")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetPriceHistory(long accountId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetPriceHistory(apprenticeshipId, new Caller(accountId, CallerType.Employer));

            return Ok(response);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}/datalocks")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLocks(long accountId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetDataLocks(apprenticeshipId, new Caller(accountId, CallerType.Employer));

            return Ok(response);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}/datalocksummary")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        [Obsolete("Use provider / employer API")]
        public async Task<IHttpActionResult> GetDataLockSummary(long accountId, long apprenticeshipId)
        {
            var response = await _apprenticeshipsOrchestrator.GetDataLockSummary(apprenticeshipId, new Caller(accountId, CallerType.Employer));

            return Ok(response);
        }

        [Route("{accountId}/apprenticeships/{apprenticeshipId}/datalocks/resolve")]
        [HttpPatch]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchDataLock(long accountId, long apprenticeshipId, [FromBody] DataLocksTriageResolutionSubmission triageSubmission)
        {
            await _apprenticeshipsOrchestrator.ResolveDataLock(apprenticeshipId, triageSubmission, new Caller(accountId, CallerType.Employer));
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}