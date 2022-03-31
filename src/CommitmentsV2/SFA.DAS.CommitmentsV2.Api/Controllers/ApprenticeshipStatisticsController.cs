using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeshipstatistics")]
    public class ApprenticeshipStatisticsController
    {
        private readonly IMediator _mediator;

        public ApprenticeshipStatisticsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        [Route("/stats")]
        public async Task<IActionResult> GetStatistics(int lastNumberOfDays)
        {
            var result = await _mediator.Send(new GetApprenticeshipStatisticsQuery { LastNumberOfDays = lastNumberOfDays });

            if (result == null)
            {
                return NotFound();
            }
        }
    }
}
