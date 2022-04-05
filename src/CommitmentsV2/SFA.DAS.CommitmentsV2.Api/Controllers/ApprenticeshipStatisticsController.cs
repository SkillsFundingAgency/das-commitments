using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeshipstatistics")]
    public class ApprenticeshipStatisticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public ApprenticeshipStatisticsController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
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

            var response = await _modelMapper.Map<GetApprenticeshipStatisticsResponse>(result);

            return Ok(response);
        }
    }
}