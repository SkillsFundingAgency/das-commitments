using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/employer")]
    public class TransferSenderController : ApiController
    {
        private readonly IEmployerOrchestrator _employerOrchestrator;

        public TransferSenderController(IEmployerOrchestrator employerOrchestrator)
        {
            _employerOrchestrator = employerOrchestrator;
        }

        [Route("{transferSenderId}/transfers/{commitmentId}", Name = "GetCommitmentForTransferSender")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> GetCommitment(long transferSenderId, long commitmentId)
        {
            var response = await _employerOrchestrator.GetCommitment(transferSenderId, commitmentId, CallerType.TransferSender);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPatch]
        [Route("{transferSenderId}/transfers/{commitmentId}/approval", Name = "PatchTransferApprovalStatus")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchTransferApprovalStatus(long transferSenderId, long commitmentId, [FromBody] TransferApprovalStatus approvalStatus, [FromBody] long TransferReceiverId)
        {
            await _employerOrchestrator.SetTransferApprovalStatus(transferSenderId, commitmentId, transferSenderId, approvalStatus);

            return Ok();
        }

    }
}
