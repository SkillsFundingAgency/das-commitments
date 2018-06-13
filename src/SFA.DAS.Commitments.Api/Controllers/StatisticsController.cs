using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Infrastructure.Authorization;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/statistics")]
    public class StatisticsController : ApiController
    {
        private readonly IStatisticsOrchestrator _statisticsOrchestrator;

        public StatisticsController(IStatisticsOrchestrator statisticsOrchestrator)
        {
            _statisticsOrchestrator = statisticsOrchestrator;
        }

        [HttpGet]
        [Route]
        [AuthorizeRemoteOnly(Roles = "Role1")]
        public async Task<IHttpActionResult> GetStatistics()
        {
            var response = await _statisticsOrchestrator.GetStatistics();

            return Ok(response);
        }
    }
}