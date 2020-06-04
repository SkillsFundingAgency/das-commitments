using System;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Infrastructure.Authorization;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/employer")]
    public class TransferController : ApiController
    {
        private readonly IEmployerOrchestrator _employerOrchestrator;

        public TransferController(IEmployerOrchestrator employerOrchestrator)
        {
            _employerOrchestrator = employerOrchestrator;
        }


        [Route("{hashedAccountId}/transfers", Name = "GetTransferRequests")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetTransferRequests(string hashedAccountId)
        {
            var response = await _employerOrchestrator.GetTransferRequests(hashedAccountId);

            return Ok(response);
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

        [Route("{transferSenderId}/sender/transfers/{transferRequestId}", Name = "GetTransferRequestForSender")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetTransferRequestForSender(long transferSenderId, long transferRequestId)
        {
            var response = await _employerOrchestrator.GetTransferRequest(transferRequestId, transferSenderId, CallerType.TransferSender);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [Route("{transferReceiverId}/receiver/transfers/{transferRequestId}", Name = "GetTransferRequestForReceiver")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetTransferRequestForReceiver(long transferReceiverId, long transferRequestId)
        {
            var response = await _employerOrchestrator.GetTransferRequest(transferRequestId, transferReceiverId, CallerType.TransferReceiver);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPatch]
        [Route("{transferSenderId}/transfers/{transferRequestId}/approval/{commitmentId}", Name = "PatchTransferRequestStatus")]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> PatchTransferApprovalStatus(long transferSenderId, long commitmentId, long transferRequestId, TransferApprovalRequest request)
        {
            await _employerOrchestrator.SetTransferApprovalStatus(transferSenderId, commitmentId, transferRequestId, request);

            return Ok();
        }
    }
}
