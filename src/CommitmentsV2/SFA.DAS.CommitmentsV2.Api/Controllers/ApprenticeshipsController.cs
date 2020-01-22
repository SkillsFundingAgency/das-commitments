using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
using SFA.DAS.CommitmentsV2.Models;
using GetApprenticeshipsFilterValuesResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsFilterValuesResponse;
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
        public async Task<IActionResult> GetApprenticeships([FromQuery]Types.Requests.GetApprenticeshipsRequest request)
        {
            try
            {
                var filterValues = new ApprenticeshipSearchFilters
                {
                    EmployerName = request.EmployerName,
                    CourseName = request.CourseName,
                    Status = request.Status,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };

                var response = await _mediator.Send(new GetApprenticeshipsQuery
                {
                    ProviderId = request.ProviderId ?? 0, 
                    PageNumber = request.PageNumber, 
                    PageItemCount = request.PageItemCount, 
                    SortField = request.SortField,
					ReverseSort = request.ReverseSort,
                    SearchFilters = filterValues
                });

                if (response == null)
                {
                    return NotFound();
                }

                var mappedApprenticeships = response.Apprenticeships.Select(x => new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse
                {
                    Id = x.Id,
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
                    TotalApprenticeshipsWithAlertsFound = response.TotalApprenticeshipsWithAlertsFound,
                    TotalApprenticeships = response.TotalApprenticeships
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("filters")]
        public async Task<IActionResult> GetApprenticeshipsFilterValues([FromQuery]long providerId)
        {
            var response = await _mediator.Send(new GetApprenticeshipsFilterValuesQuery { ProviderId = providerId });

            if (response == null)
            {
                return NotFound();
            }

            return Ok(new GetApprenticeshipsFilterValuesResponse
            {
                    EmployerNames = response.EmployerNames,
                    CourseNames = response.CourseNames,
                    Statuses = response.Statuses,
                    StartDates = response.StartDates,
                    EndDates = response.EndDates
            });
        }
    }
}