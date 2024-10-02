using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails;
using SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping;
using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/cohorts/{cohortId:long}/draft-apprenticeships")]
[ApiController]
[Authorize]
public class DraftApprenticeshipController(
    IMediator mediator,
    IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> updateDraftApprenticeshipMapper,
    IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse> getDraftApprenticeshipMapper,
    IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> addDraftApprenticeshipMapper,
    IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse> getDraftApprenticeshipsResultMapper,
    IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand> deleteDraftApprenticeshipsMapper)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAll(long cohortId)
    {
        var result = await mediator.Send(new GetDraftApprenticeshipsQuery(cohortId));
        var response = await getDraftApprenticeshipsResultMapper.Map(result);

        if (response.DraftApprenticeships == null)
        {
            return NotFound();
        }
        
        return Ok(response);
    }

    [HttpGet]
    [Route("{apprenticeshipId:long}")]
    public async Task<IActionResult> Get(long cohortId, long apprenticeshipId)
    {
        var command = new GetDraftApprenticeshipQuery(cohortId, apprenticeshipId);

        var response = await mediator.Send(command);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(await getDraftApprenticeshipMapper.Map(response));
    }

    [HttpGet]
    [Route("{apprenticeshipId:long}/prior-learning-summary")]
    public async Task<IActionResult> GetApprenticeshipPriorLearningSummary(long cohortId, long apprenticeshipId)
    {
        var query = new GetDraftApprenticeshipPriorLearningSummaryQuery(cohortId, apprenticeshipId);

        var response = await mediator.Send(query);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(new GetDraftApprenticeshipPriorLearningSummaryResponse
        {
            ApprenticeshipId = apprenticeshipId,
            CohortId = cohortId,
            TrainingTotalHours = response.TrainingTotalHours,
            DurationReducedByHours = response.DurationReducedByHours,
            PriceReducedBy = response.PriceReducedBy,
            PercentageOfPriorLearning = response.PercentageOfPriorLearning,
            MinimumPercentageReduction = response.MinimumPercentageReduction,
            MinimumPriceReduction = response.MinimumPriceReduction,
            RplPriceReductionError = response.RplPriceReductionError,
            FundingBandMaximum = response.FundingBandMaximum
        }); 
    }
    
    [HttpPut]
    [Route("{apprenticeshipId:long}")]
    public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody]UpdateDraftApprenticeshipRequest request)
    {
        var command = await updateDraftApprenticeshipMapper.Map(request);
        command.CohortId = cohortId;
        command.ApprenticeshipId = apprenticeshipId;

        await mediator.Send(command);

        return Ok();
    }

    [HttpPost]
    [Route("{apprenticeshipId:long}/recognise-prior-learning")]
    public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody] RecognisePriorLearningRequest request)
    {
        await mediator.Send(new RecognisePriorLearningCommand
        {
            ApprenticeshipId = apprenticeshipId, 
            CohortId = cohortId,
            RecognisePriorLearning = request.RecognisePriorLearning, 
            UserInfo = request.UserInfo
        });
        
        return Ok();
    }

    [HttpPost]
    [Route("{apprenticeshipId:long}/prior-learning")]
    public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody] PriorLearningDetailsRequest request)
    {
        await mediator.Send(new PriorLearningDetailsCommand
        {
            ApprenticeshipId = apprenticeshipId,
            CohortId = cohortId,
            DurationReducedBy = request.DurationReducedBy,
            PriceReducedBy = request.PriceReducedBy,
            DurationReducedByHours = request.DurationReducedByHours,
            Rpl2Mode = request.Rpl2Mode,
            UserInfo = request.UserInfo
        });
        
        return Ok();
    }


    [HttpPost]
    [Route("{apprenticeshipId:long}/prior-learning-data")]
    public async Task<IActionResult> UpdateRplData(long cohortId, long apprenticeshipId, [FromBody] PriorLearningDataRequest request)
    {
        await mediator.Send(new PriorLearningDataCommand
        {
            ApprenticeshipId = apprenticeshipId,
            CohortId = cohortId,
            DurationReducedBy = request.DurationReducedBy,
            PriceReducedBy = request.PriceReducedBy,
            DurationReducedByHours = request.DurationReducedByHours,
            IsDurationReducedByRpl = request.IsDurationReducedByRpl,
            TrainingTotalHours = request.TrainingTotalHours,
            UserInfo = request.UserInfo
        });
        
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Add(long cohortId, [FromBody]AddDraftApprenticeshipRequest request)
    {
        var command = await addDraftApprenticeshipMapper.Map(request);

        command.CohortId = cohortId;
            
        var result = await mediator.Send(command);
            
        return Ok(new AddDraftApprenticeshipResponse
        {
            DraftApprenticeshipId = result.Id
        });
    }

    [HttpPost]
    [Route("{apprenticeshipId:long}")]
    public async Task<IActionResult> Delete(long cohortId, long apprenticeshipId, [FromBody]DeleteDraftApprenticeshipRequest request)
    {
        var command = await deleteDraftApprenticeshipsMapper.Map(request);
        command.CohortId = cohortId;
        command.ApprenticeshipId = apprenticeshipId;

        await mediator.Send(command);

        return Ok();
    }
}