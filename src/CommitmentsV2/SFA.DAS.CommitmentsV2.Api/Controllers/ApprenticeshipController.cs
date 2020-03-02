using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
using SFA.DAS.CommitmentsV2.Models;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;

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
                var filterValues = new ApprenticeshipSearchFilters
                {
                    SearchTerm = request.SearchTerm,
                    EmployerName = request.EmployerName,
                    CourseName = request.CourseName,
                    Status = request.Status,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };

                var queryResult = await _mediator.Send(new GetApprenticeshipsQuery
                {
                    ProviderId = request.ProviderId ?? 0,
                    PageNumber = request.PageNumber,
                    PageItemCount = request.PageItemCount,
                    SortField = request.SortField,
                    ReverseSort = request.ReverseSort,
                    SearchFilters = filterValues
                });

                if (queryResult == null)
                {
                    return NotFound();
                }

                var response = await _modelMapper.Map<GetApprenticeshipsResponse>(queryResult);

                return Ok(response);
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

            return Ok(response);
        }
    }
}
