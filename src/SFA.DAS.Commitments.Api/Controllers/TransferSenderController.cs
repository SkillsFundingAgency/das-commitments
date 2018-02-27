using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Domain;

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
        
    }
}
