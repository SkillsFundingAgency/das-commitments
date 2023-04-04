using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping;

using GetDraftApprenticeshipResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses.GetDraftApprenticeshipResponse;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningDetails;
using SFA.DAS.CommitmentsV2.Application.Commands.RecognisePriorLearning;
using SFA.DAS.CommitmentsV2.Application.Commands.PriorLearningData;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/cohorts/{cohortId}/draft-apprenticeships")]
    [ApiController]
    [Authorize]
    public class DraftApprenticeshipController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> _updateDraftApprenticeshipMapper;
        private readonly IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse> _getDraftApprenticeshipMapper;
        private readonly IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> _addDraftApprenticeshipMapper;
        private readonly IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse> _getDraftApprenticeshipsResultMapper;
        private readonly IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand> _deleteDraftApprenticeshipsMapper;

        public DraftApprenticeshipController(
            IMediator mediator,
            IOldMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand> updateDraftApprenticeshipMapper,
            IOldMapper<GetDraftApprenticeshipQueryResult, GetDraftApprenticeshipResponse> getDraftApprenticeshipMapper,
            IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand> addDraftApprenticeshipMapper, 
            IOldMapper<GetDraftApprenticeshipsQueryResult, GetDraftApprenticeshipsResponse> getDraftApprenticeshipsResultMapper,
            IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand> deleteDraftApprenticeshipsMapper
            )
        {
            _mediator = mediator;
            _updateDraftApprenticeshipMapper = updateDraftApprenticeshipMapper;
            _getDraftApprenticeshipMapper = getDraftApprenticeshipMapper;
            _addDraftApprenticeshipMapper = addDraftApprenticeshipMapper;
            _getDraftApprenticeshipsResultMapper = getDraftApprenticeshipsResultMapper;
            _deleteDraftApprenticeshipsMapper = deleteDraftApprenticeshipsMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(long cohortId)
        {
            var result = await _mediator.Send(new GetDraftApprenticeshipsQuery(cohortId));
            var response = await _getDraftApprenticeshipsResultMapper.Map(result);

            if (response.DraftApprenticeships == null)
            {
                return NotFound();
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Get(long cohortId, long apprenticeshipId)
        {
            var command = new GetDraftApprenticeshipQuery(cohortId, apprenticeshipId);

            var response = await _mediator.Send(command);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(await _getDraftApprenticeshipMapper.Map(response));
        }

        [HttpPut]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody]UpdateDraftApprenticeshipRequest request)
        {
            var command = await _updateDraftApprenticeshipMapper.Map(request);
            command.CohortId = cohortId;
            command.ApprenticeshipId = apprenticeshipId;

            await _mediator.Send(command);

            return Ok();
        }

        [HttpPost]
        [Route("{apprenticeshipId}/recognise-prior-learning")]
        public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody] RecognisePriorLearningRequest request)
        {
            await _mediator.Send(new RecognisePriorLearningCommand
            {
                ApprenticeshipId = apprenticeshipId, 
                CohortId = cohortId,
                RecognisePriorLearning = request.RecognisePriorLearning, 
                UserInfo = request.UserInfo
            });
            return Ok();
        }

        [HttpPost]
        [Route("{apprenticeshipId}/prior-learning")]
        public async Task<IActionResult> Update(long cohortId, long apprenticeshipId, [FromBody] PriorLearningDetailsRequest request)
        {
            await _mediator.Send(new PriorLearningDetailsCommand
            {
                ApprenticeshipId = apprenticeshipId,
                CohortId = cohortId,
                DurationReducedBy = request.DurationReducedBy,
                PriceReducedBy = request.PriceReducedBy,
                DurationReducedByHours = request.DurationReducedByHours,
                WeightageReducedBy = request.WeightageReducedBy,
                QualificationsForRplReduction = request.QualificationsForRplReduction,
                ReasonForRplReduction = request.ReasonForRplReduction,
                Rpl2Mode = request.Rpl2Mode,
                UserInfo = request.UserInfo
            });
            return Ok();
        }


        [HttpPost]
        [Route("{apprenticeshipId}/prior-learning-data")]
        public async Task<IActionResult> UpdateRplData(long cohortId, long apprenticeshipId, [FromBody] PriorLearningDataRequest request)
        {
            await _mediator.Send(new PriorLearningDataCommand
            {
                ApprenticeshipId = apprenticeshipId,
                CohortId = cohortId,
                DurationReducedBy = request.DurationReducedBy,
                PriceReducedBy = request.PriceReducedBy,
                DurationReducedByHours = request.DurationReducedByHours,
                WeightageReducedBy = request.WeightageReducedBy,
                QualificationsForRplReduction = request.QualificationsForRplReduction,
                ReasonForRplReduction = request.ReasonForRplReduction,
                IsDurationReducedByRpl = request.IsDurationReducedByRpl,
                TrainingTotalHours = request.TrainingTotalHours,
                CostBeforeRpl = request.CostBeforeRpl,
                Rpl2Mode = request.Rpl2Mode,
                UserInfo = request.UserInfo
            });
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Add(long cohortId, [FromBody]AddDraftApprenticeshipRequest request)
        {
            var command = await _addDraftApprenticeshipMapper.Map(request);

            command.CohortId = cohortId;
            
            var result = await _mediator.Send(command);
            
            return Ok(new AddDraftApprenticeshipResponse
            {
                DraftApprenticeshipId = result.Id
            });
        }

        [HttpPost]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Delete(long cohortId, long apprenticeshipId, [FromBody]DeleteDraftApprenticeshipRequest request)
        {
            var command = await _deleteDraftApprenticeshipsMapper.Map(request);
            command.CohortId = cohortId;
            command.ApprenticeshipId = apprenticeshipId;

            await _mediator.Send(command);

            return Ok();
        }
    }
}