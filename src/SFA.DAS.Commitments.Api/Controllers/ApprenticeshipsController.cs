using System;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.DataLock;
using System.Net;

using DataLocksTriageResolutionSubmission = SFA.DAS.Commitments.Api.Types.DataLock.DataLocksTriageResolutionSubmission;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api")]
    public class ApprenticeshipsController : ApiController
    {
        private readonly ApprenticeshipsOrchestrator _orchestrator;

        public ApprenticeshipsController(ApprenticeshipsOrchestrator orchestrator)
        {
            if(orchestrator==null)
                throw new ArgumentNullException(nameof(ApprenticeshipsOrchestrator));

            _orchestrator = orchestrator;
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks/{dataLockEventId}")]
        [Authorize(Roles = "Role1")]
        [Obsolete("Not in use")]
        public async Task<IHttpActionResult> GetDataLock(long apprenticeshipId, long dataLockEventId)
        {
            var response = await _orchestrator.GetDataLock(apprenticeshipId, dataLockEventId);

            return Ok(response.Data);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks")]
        [Authorize(Roles = "Role1")]
        [Obsolete("Use provider / employer API")]
        public async Task<IHttpActionResult> GetDataLocks(long apprenticeshipId)
        {
            var response = await _orchestrator.GetDataLocks(apprenticeshipId);

            return Ok(response);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocksummary")]
        [Authorize(Roles = "Role1")]
        [Obsolete("Use provider / employer API")]
        public async Task<IHttpActionResult> GetDataLockSummary(long apprenticeshipId)
        {
            var response = await _orchestrator.GetDataLockSummary(apprenticeshipId);

            return Ok(response);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks/{dataLockEventId}")]
        [HttpPatch]
        [Authorize(Roles = "Role1")]
        [Obsolete("Use provider API")]
        public async Task<IHttpActionResult> PatchDataLock(long apprenticeshipId, long dataLockEventId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _orchestrator.TriageDataLock(apprenticeshipId, dataLockEventId, triageSubmission);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks")]
        [HttpPatch]
        [Authorize(Roles = "Role1")]
        [Obsolete("Use provider API")]
        public async Task<IHttpActionResult> PatchDataLock(long apprenticeshipId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _orchestrator.TriageDataLocks(apprenticeshipId, triageSubmission);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks/resolve")]
        [HttpPatch]
        [Authorize(Roles = "Role1")]
        [Obsolete("Use Employer API")]
        public async Task<IHttpActionResult> PatchDataLock(long apprenticeshipId, [FromBody] DataLocksTriageResolutionSubmission triageSubmission)
        {
            await _orchestrator.ResolveDataLock(apprenticeshipId, triageSubmission);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [Route("apprenticeships/{apprenticeshipId}/prices")]
        [Authorize(Roles = "Role1")]
        [Obsolete("Use Provider/Employer API")]
        public async Task<IHttpActionResult> GetPriceHistory(long apprenticeshipId)
        {
            var response = await _orchestrator.GetPriceHistory(apprenticeshipId);

            return Ok(response.Data);
        }
    }
}
