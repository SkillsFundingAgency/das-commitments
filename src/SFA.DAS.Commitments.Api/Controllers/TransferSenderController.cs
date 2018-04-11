using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Infrastructure.Authorization;

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
        [AuthorizeRemoteOnly(Roles = "Role1")]
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
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchTransferApprovalStatus(long transferSenderId, long commitmentId, TransferApprovalRequest request)
        {
            await _employerOrchestrator.SetTransferApprovalStatus(transferSenderId, commitmentId, request);

            return Ok();
        }

    }
}
