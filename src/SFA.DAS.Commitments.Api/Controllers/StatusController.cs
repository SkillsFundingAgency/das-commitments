using System;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/status")]
    public class StatusController : ApiController
    {
        private readonly EmployerOrchestrator _orchestrator;

        public StatusController(EmployerOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Route("")]
        [Authorize(Roles = "Role1")]
        public async Task<IHttpActionResult> Index()
        {
            try
            {
                // Do some Infrastructre work here to smoke out any issues.
                await _orchestrator.GetApprenticeship(1, 1);
                return Ok();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}
