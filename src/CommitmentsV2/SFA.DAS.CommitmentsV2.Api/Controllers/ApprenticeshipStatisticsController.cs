using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeshipstatistics")]
    public class ApprenticeshipStatisticsController
    {

        [HttpGet]
        [Route("/stats")]
        public async Task<IActionResult> GetStatistics(int lastNumberOfDays)
        {

        }
    }
}
