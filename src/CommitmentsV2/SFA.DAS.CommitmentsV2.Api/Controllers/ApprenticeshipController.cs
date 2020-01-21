using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
<<<<<<< HEAD:src/CommitmentsV2/SFA.DAS.CommitmentsV2.Api/Controllers/ApprenticeshipsController.cs
using SFA.DAS.CommitmentsV2.Types;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
=======
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
>>>>>>> manage_apprentices:src/CommitmentsV2/SFA.DAS.CommitmentsV2.Api/Controllers/ApprenticeshipController.cs

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships")]
    public class ApprenticeshipController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;
        private readonly ILogger<ApprenticeshipController> _logger;

        public ApprenticeshipController(IMediator mediator, IModelMapper modelMapper, ILogger<ApprenticeshipController> logger)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
            _logger = logger;
        }

        [HttpGet]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Get(long apprenticeshipId)
        {
            var query = new GetApprenticeshipQuery(apprenticeshipId);
            var result = await _mediator.Send(query);

            if (result == null)  {  return NotFound(); }

            var response = await _modelMapper.Map<GetApprenticeshipResponse>(result);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetApprenticeships([FromQuery]Types.Requests.GetApprenticeshipsRequest request)
        {
            try
            {
                var response = await _mediator.Send(new GetApprenticeshipsQuery
                {
                    ProviderId = request.ProviderId ?? 0,
                    PageNumber = request.PageNumber,
                    PageItemCount = request.PageItemCount,
                    SortField = request.SortField,
                    ReverseSort = request.ReverseSort
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
                    ApprenticeshipStatus = x.ApprenticeshipStatus == ApprenticeshipStatus.WaitingToStart ? "Waiting to Start" : x.ApprenticeshipStatus.ToString(),
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
        public async Task<IActionResult> GetApprenticeshipsFilterValues(long providerId)
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
