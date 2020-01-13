using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApprenticeshipsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ApprenticeshipsController> _logger;

        public ApprenticeshipsController(IMediator mediator, ILogger<ApprenticeshipsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Route("{providerId}")]
        public async Task<IActionResult> GetApprenticeships(uint providerId, [FromQuery]int pageNumber = 0, [FromQuery]int pageItemCount = 0, [FromQuery]string sortField = "", [FromQuery]bool reverseSort = false)
        {
            try
            {
                var response = await _mediator.Send(new GetApprenticeshipsRequest
                {
                    ProviderId = providerId, 
                    PageNumber = pageNumber, 
                    PageItemCount = pageItemCount, 
                    SortField = sortField,
					ReverseSort = reverseSort
                });

                if (response == null)
                {
                    return NotFound();
                }

                //TODO: Remove this mapping once we have consolidated the old Types with the new API types
                var mappedApprenticeships = response.Apprenticeships.Select(x => new ApprenticeshipDetailsResponse
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Uln = x.Uln,
                    EmployerName = x.EmployerName,
                    CourseName = x.CourseName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    PaymentStatus = x.PaymentStatus,
                    Alerts = x.Alerts
                });

                return Ok(new GetApprenticeshipsResponse
                {
                    Apprenticeships = mappedApprenticeships,
                    TotalApprenticeshipsFound = response.TotalApprenticeshipsFound,
                    TotalApprenticeshipsWithAlertsFound = response.TotalApprenticeshipsWithAlertsFound
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("filters/{providerId}")]
        public async Task<IActionResult> GetApprenticeshipsFilterValues(uint providerId)
        {
            var response = await _mediator.Send(new GetApprenticeshipsFilterValuesQuery { ProviderId = providerId });

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}