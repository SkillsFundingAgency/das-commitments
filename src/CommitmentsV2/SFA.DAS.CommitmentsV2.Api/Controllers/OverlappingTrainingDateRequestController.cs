using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.ResolveOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateChangeOfEmployerOverlap;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOverlap;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/overlapping-training-date-request")]
public class OverlappingTrainingDateRequestController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpPost]
    [Route("{providerId:long}/create")]
    public async Task<IActionResult> CreateOverlappingTrainingDate(long providerId, [FromBody] CreateOverlappingTrainingDateRequest request)
    {
        var result = await mediator.Send(new CreateOverlappingTrainingDateRequestCommand
        {
            ProviderId = providerId,
            DraftApprenticeshipId = request.DraftApprenticeshipId,
            UserInfo = request.UserInfo
        });

        return Ok(new CreateOverlappingTrainingDateResponse { Id = result.Id });
    }

    [HttpPost]
    [Route("{providerId:long}/validate")]
    public async Task<IActionResult> ValidateDraftApprenticeship(long providerId, [FromBody] ValidateDraftApprenticeshipRequest request)
    {
        var command = new ValidateDraftApprenticeshipDetailsCommand { DraftApprenticeshipRequest = request };
        await mediator.Send(command);
        
        return Ok();
    }

    [HttpPost]
    [Route("{providerId:long}/validateChangeOfEmployerOverlap")]
    public async Task<IActionResult> ValidateChangeOfEmployerOverlap(long providerId, [FromBody] ValidateChangeOfEmployerOverlapRequest request)
    {
        var command = new ValidateChangeOfEmployerOverlapCommand { ProviderId = providerId, Uln = request.Uln, StartDate = request.StartDate, EndDate = request.EndDate };
        await mediator.Send(command);
        return Ok();
    }

    [HttpGet]
    [Route("{providerId:long}/validateUlnOverlap")]
    public async Task<IActionResult> ValidateUlnOverlapOnStartDate(long providerId, string uln, string startDate, string endDate)
    {
        var query = new ValidateUlnOverlapOnStartDateQuery { ProviderId = providerId, Uln = uln, StartDate = startDate, EndDate = endDate };
        var result = await mediator.Send(query);
        
        return Ok(new ValidateUlnOverlapOnStartDateResponse
        {
            HasOverlapWithApprenticeshipId = result.HasOverlapWithApprenticeshipId,
            HasStartDateOverlap = result.HasStartDateOverlap
        });
    }

    [HttpGet]
    [Route("{draftApprenticeshipId:long}/getOverlapRequest")]
    public async Task<IActionResult> GetPendingOverlappingTrainingDateRequest(long draftApprenticeshipId)
    {
        var result = await mediator.Send(new GetPendingOverlapRequestsQuery(draftApprenticeshipId));

        return Ok(new GetOverlapRequestsResponse
        {
            DraftApprenticeshipId = result?.DraftApprenticeshipId,
            PreviousApprenticeshipId = result?.PreviousApprenticeshipId,
            CreatedOn = result?.CreatedOn
        });
    }

    [HttpGet]
    [Route("{apprenticeshipId:long}")]
    public async Task<IActionResult> Get(long apprenticeshipId)
    {
        var query = new GetOverlappingTrainingDateRequestQuery(apprenticeshipId);
        var result = await mediator.Send(query);

        if (result == null)
        {
            return Ok(null);
        }

        var response = await modelMapper.Map<GetOverlappingTrainingDateRequestResponce>(result);
        
        return Ok(response);
    }

    [HttpPost]
    [Route("resolve")]
    public async Task<IActionResult> Resolve([FromBody] ResolveApprenticeshipOverlappingTrainingDateRequest request)
    {
        await mediator.Send(new ResolveOverlappingTrainingDateRequestCommand
        {
            ApprenticeshipId = request.ApprenticeshipId,
            ResolutionType = request.ResolutionType,
        });

        return Ok();
    }

    [HttpGet]
    [Route("{draftApprenticeshipId:long}/validateEmailOverlap")]
    public async Task<IActionResult> ValidateEmailOverlap(long draftApprenticeshipId, long cohortId, string Email, string startDate, string endDate)
    {
        var query = new ValidateEmailOverlapQuery { DraftApprenticeshipId = draftApprenticeshipId, Email = Email, StartDate = startDate, EndDate = endDate, CohortId = cohortId };
        var result = await mediator.Send(query);

        return Ok(result);
    }
}