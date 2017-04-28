using System;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.DataLock;

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
        //[Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLock(long apprenticeshipId, long dataLockEventId)
        {
            var response = await _orchestrator.GetDataLock(apprenticeshipId, dataLockEventId);

            return Ok(response.Data);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks")]
        //[Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetDataLocks(long apprenticeshipId)
        {
            var response = await _orchestrator.GetDataLocks(apprenticeshipId);

            return Ok(response.Data);
        }

        [Route("apprenticeships/{apprenticeshipId}/datalocks/{dataLockEventId}")]
        //[Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchDataLock(long apprenticeshipId, long dataLockEventId, [FromBody] DataLockTriageSubmission triageSubmission)
        {
            await _orchestrator.PatchDataLock(apprenticeshipId, dataLockEventId, triageSubmission);
            return Ok();
        }
    }
}
