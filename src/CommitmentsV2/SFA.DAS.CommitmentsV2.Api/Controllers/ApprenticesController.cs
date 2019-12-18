using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;

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

        [Authorize]
        [HttpGet]
        [Route("{providerId}")]
        public async Task<IActionResult> GetApprovedApprentices(uint providerId)
        {
            try
            {
                var response = await _mediator.Send(new GetApprenticeshipsRequest {ProviderId = providerId});

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
    }
}