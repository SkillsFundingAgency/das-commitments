using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
using SFA.DAS.CommitmentsV2.Models;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;

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

            if (result == null) { return NotFound(); }

            var response = await _modelMapper.Map<GetApprenticeshipResponse>(result);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetApprenticeships([FromQuery] GetApprenticeshipsRequest request)
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
                    EndDate = request.EndDate,
                    ProviderName = request.ProviderName,
                    AccountLegalEntityId = request.AccountLegalEntityId,
                    StartDateRange = new DateRange { From = request.StartDateRangeFrom, To = request.StartDateRangeTo }
                };

                var queryResult = await _mediator.Send(new GetApprenticeshipsQuery
                {
                    EmployerAccountId = request.AccountId,
                    ProviderId = request.ProviderId,
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
        public async Task<IActionResult> GetApprenticeshipsFilterValues([FromQuery] GetApprenticeshipFiltersRequest request)
        {
            var response = await _mediator.Send(new GetApprenticeshipsFilterValuesQuery { ProviderId = request.ProviderId, EmployerAccountId = request.EmployerAccountId });

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("details/editenddate")]
        public async Task<IActionResult> EditEndDate([FromBody]EditEndDateRequest request)
        {
            var response = await _mediator.Send(new EditEndDateRequestCommand
            {
                ApprenticeshipId = request.ApprenticeshipId,
                EndDate = request.EndDate,
                UserInfo = request.UserInfo
            });

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        
        [HttpPost]
        [Route("{apprenticeshipId}/stop")]
        public async Task<IActionResult> StopApprenticeship(long apprenticeshipId, [FromBody] StopApprenticeshipRequest request)
        {
            await _mediator.Send(new StopApprenticeshipCommand(
                request.AccountId,
                apprenticeshipId,
                request.StopDate,
                request.MadeRedundant,
                request.UserInfo));

            return Ok();

        }

        [HttpPost]
        [Route("details/pause")]
        public async Task<IActionResult> Pause([FromBody]PauseApprenticeshipRequest request)
        {
            var response = await _mediator.Send(new PauseApprenticeshipCommand
            {
                ApprenticeshipId = request.ApprenticeshipId,
                UserInfo = request.UserInfo
            });

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
        
        [HttpPost]
        [Route("details/resume")]
        public async Task<IActionResult> Resume([FromBody] ResumeApprenticeshipRequest request)
        {
            var response = await _mediator.Send(new ResumeApprenticeshipCommand
            {
                ApprenticeshipId = request.ApprenticeshipId,
                UserInfo = request.UserInfo
            });
            return Ok(response);
        }

        [HttpPut]
        [Route("{apprenticeshipId}/stopdate")]
        public async Task<IActionResult> UpdateApprenticeshipStopDate(long apprenticeshipId, [FromBody] ApprenticeshipStopDateRequest request)
        {   
            var response = await _mediator.Send(new UpdateApprenticeshipStopDateCommand(            
                request.AccountId,
                apprenticeshipId,
                request.NewStopDate,
                request.UserInfo
            ));
			if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }
		
        [HttpPost]
        [Route("edit/validate")]
        public async Task<IActionResult> ValidateApprenticeshipForEdit([FromBody] ValidateApprenticeshipForEditRequest request)
        {
            var command = await _modelMapper.Map<ValidateApprenticeshipForEditCommand>(request);
            var response = await _mediator.Send(command);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

    }
}
