using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApprenticesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticesController> _logger;

        public ApprenticesController(IMediator mediator, ILogger<ApprenticesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        
        [HttpGet]
        [Route("{providerId}")]
        public async Task<IActionResult> GetApprovedApprentices(uint providerId)
        {
            try
            {
                var response = await _mediator.Send(new GetApprovedApprenticesRequest {ProviderId = providerId});

                if (response == null)
                {
                    return NotFound();
                }

                return Ok(response.Apprenticeships);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("filters/{providerId}")]
        public async Task<IActionResult> GetApprovedApprenticesFilterValues(uint providerId)
        {
            var response = await _mediator.Send(new GetApprovedApprenticesFilterValuesQuery { ProviderId = providerId });

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}