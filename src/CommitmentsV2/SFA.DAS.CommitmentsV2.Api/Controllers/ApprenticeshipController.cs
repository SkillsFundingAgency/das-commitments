using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation;
using SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateUln;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using EditApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.EditApprenticeshipResponse;
using GetApprenticeshipsResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetApprenticeshipsResponse;
using IAuthenticationService = SFA.DAS.CommitmentsV2.Authentication.IAuthenticationService;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/apprenticeships")]
public class ApprenticeshipController(
    IMediator mediator,
    IModelMapper modelMapper,
    IAuthenticationService authenticationService,
    ILogger<ApprenticeshipController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route("{apprenticeshipId:long}")]
    public async Task<IActionResult> Get(long apprenticeshipId)
    {
        var query = new GetApprenticeshipQuery(apprenticeshipId);
        var result = await mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        var response = await modelMapper.Map<GetApprenticeshipResponse>(result);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetApprenticeships([FromQuery] GetApprenticeshipsRequest request)
    {
        var logText = request.AccountId != null
            ? "Employer account Id :" + (request.AccountId ?? 0)
            : ", Provider Id :" + (request.ProviderId ?? 0);

        logger.LogInformation("Get apprenticeships for : {Text}.", logText);

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
                StartDateRange = new DateRange { From = request.StartDateRangeFrom, To = request.StartDateRangeTo },
                Alert = request.Alert,
                ApprenticeConfirmationStatus = request.ApprenticeConfirmationStatus,
                DeliveryModel = request.DeliveryModel,
                IsOnFlexiPaymentPilot = request.IsOnFlexiPaymentPilot
            };

            var queryResult = await mediator.Send(new GetApprenticeshipsQuery
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

            var response = await modelMapper.Map<GetApprenticeshipsResponse>(queryResult);

            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception thrown in {MethodName}.", nameof(GetApprenticeships));
            throw;
        }
    }

    [HttpGet]
    [Route("filters")]
    public async Task<IActionResult> GetApprenticeshipsFilterValues([FromQuery] GetApprenticeshipFiltersRequest request)
    {
        var response = await mediator.Send(new GetApprenticeshipsFilterValuesQuery
        {
            ProviderId = request.ProviderId,
            EmployerAccountId = request.EmployerAccountId
        });

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("details/editenddate")]
    public async Task<IActionResult> EditEndDate([FromBody] EditEndDateRequest request)
    {
        logger.LogInformation("Edit end date apprenticeship api endpoint called for : {Id}.", request.ApprenticeshipId);

        await mediator.Send(new EditEndDateRequestCommand
        {
            ApprenticeshipId = request.ApprenticeshipId,
            EndDate = request.EndDate,
            UserInfo = request.UserInfo
        });

        return Ok();
    }

    [HttpPost]
    [Route("{apprenticeshipId:long}/stop")]
    public async Task<IActionResult> StopApprenticeship(long apprenticeshipId, [FromBody] StopApprenticeshipRequest request)
    {
        logger.LogInformation("Stop apprenticeship api endpoint called for : {Id}.", apprenticeshipId);

        var party = authenticationService.GetUserParty();

        await mediator.Send(new StopApprenticeshipCommand(
            request.AccountId,
            apprenticeshipId,
            request.StopDate,
            request.MadeRedundant,
            request.UserInfo,
            party));

        return Ok();
    }

    [HttpPost]
    [Route("details/pause")]
    public async Task<IActionResult> Pause([FromBody] PauseApprenticeshipRequest request)
    {
        logger.LogInformation("Pause apprenticeship api endpoint called for : {Id}.", request.ApprenticeshipId);

        await mediator.Send(new PauseApprenticeshipCommand
        {
            ApprenticeshipId = request.ApprenticeshipId,
            UserInfo = request.UserInfo
        });

        return Ok();
    }

    [HttpPost]
    [Route("details/resume")]
    public async Task<IActionResult> Resume([FromBody] ResumeApprenticeshipRequest request)
    {
        logger.LogInformation("Resume apprenticeship api endpoint called for : {Id}.", request.ApprenticeshipId);

        await mediator.Send(new ResumeApprenticeshipCommand
        {
            ApprenticeshipId = request.ApprenticeshipId,
            UserInfo = request.UserInfo
        });

        return Ok();
    }

    [HttpPut]
    [Route("{apprenticeshipId:long}/stopdate")]
    public async Task<IActionResult> UpdateApprenticeshipStopDate(long apprenticeshipId,
        [FromBody] ApprenticeshipStopDateRequest request)
    {
        logger.LogInformation("Update apprenticeship stop date api endpoint called for : {Id}.", apprenticeshipId);

        await mediator.Send(new UpdateApprenticeshipStopDateCommand(
            request.AccountId,
            apprenticeshipId,
            request.NewStopDate,
            request.UserInfo
        ));

        return Ok();
    }

    [HttpPost]
    [Route("{apprenticeshipId:long}/resendinvitation")]
    public async Task<IActionResult> ResendInvitation(long apprenticeshipId, [FromBody] SaveDataRequest request)
    {
        logger.LogInformation("Resend invitation email for : {Id}.", apprenticeshipId);
        
        await mediator.Send(new ResendInvitationCommand(apprenticeshipId, request.UserInfo));
        
        return Accepted();
    }

    [HttpPost]
    [Route("edit/validate")]
    public async Task<IActionResult> ValidateApprenticeshipForEdit([FromBody] ValidateApprenticeshipForEditRequest request)
    {
        var command = await modelMapper.Map<ValidateApprenticeshipForEditCommand>(request);
        var response = await mediator.Send(command);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("edit")]
    public async Task<IActionResult> EditApprenticeship([FromBody] EditApprenticeshipApiRequest request)
    {
        logger.LogInformation("Edit apprenticeship api endpoint called for : {Id}.", request.ApprenticeshipId);

        var command = new EditApprenticeshipCommand { EditApprenticeshipRequest = request };
        var response = await mediator.Send(command);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(new EditApprenticeshipResponse
            { ApprenticeshipId = response.ApprenticeshipId, NeedReapproval = response.NeedReapproval });
    }

    [HttpPost]
    [Route("uln/validate")]
    public async Task<IActionResult> ValidateUlnOverlap([FromBody] ValidateUlnOverlapRequest request)
    {
        var command = new ValidateUlnOverlapCommand
        {
            ULN = request.ULN,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ApprenticeshipId = request.ApprenticeshipId
        };

        var response = await mediator.Send(command);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet]
    [Route("validate")]
    public async Task<IActionResult> ValidateApprenticeship([FromQuery] string firstName, [FromQuery] string lastName, [FromQuery] DateTime dateOfBirth)
    {
        var query = new GetApprenticeshipsValidateQuery(firstName, lastName, dateOfBirth);

        var result = await mediator.Send(query);

        return Ok(result);
    }

    [HttpGet]
    [Route("{apprenticeshipId:long}/approved-apprenticeship")]
    public async Task<IActionResult> GetApprovedApprenticeship(long apprenticeshipId)
    {
        var query = new GetSupportApprovedApprenticeshipsQuery(null, null, apprenticeshipId);
        var result = await mediator.Send(query);
        if(!result.ApprovedApprenticeships.Any())
            return NotFound();

        return Ok(result.ApprovedApprenticeships.First());
    }

    [HttpGet]
    [Route("uln/{uln:long}/approved-apprenticeships")]
    public async Task<IActionResult> GetApprovedApprenticeshipForUln(string uln)
    {
        var query = new GetSupportApprovedApprenticeshipsQuery(null, uln.ToString(), null);
        var result = await mediator.Send(query);

        return Ok(result);
    }
}