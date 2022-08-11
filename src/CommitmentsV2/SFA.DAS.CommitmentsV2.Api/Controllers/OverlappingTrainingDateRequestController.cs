using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/overlapping-training-date-request")]
    public class OverlappingTrainingDateRequestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public OverlappingTrainingDateRequestController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpPost]
        [Route("{providerId}/create")]
        public async Task<IActionResult> CreateOverlappingTrainingDate(long providerId, [FromBody] CreateOverlappingTrainingDateRequest request)
        {
            var result = await _mediator.Send(new CreateOverlappingTrainingDateRequestCommand
            {
                ProviderId = providerId,
                DraftApprneticeshipId = request.DraftApprenticeshipId,
                UserInfo = request.UserInfo
            });

            return Ok(new CreateOverlappingTrainingDateResponse { Id = result.Id });
        }

        [HttpPost]
        [Route("{providerId}/validate")]
        public async Task<IActionResult> ValidateDraftApprenticeship(long providerId, [FromBody] ValidateDraftApprenticeshipRequest request)
        {
            var command = new ValidateDraftApprenticeshipDetailsCommand { DraftApprenticeshipRequest = request };
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet]
        [Route("{providerId}/validateUlnOverlap")]
        public async Task<IActionResult> ValidateUlnOverlapOnStartDate(long providerId, string uln, string startDate, string endDate)
        {
            var query = new ValidateUlnOverlapOnStartDateQuery { ProviderId = providerId, Uln = uln, StartDate = startDate, EndDate = endDate };
            var result = await _mediator.Send(query);
            return Ok(new ValidateUlnOverlapOnStartDateResponse
            {
                HasOverlapWithApprenticeshipId = result.HasOverlapWithApprenticeshipId,
                HasStartDateOverlap = result.HasStartDateOverlap
            });
        }

        [HttpGet]
        [Route("{apprenticeshipId}")]
        public async Task<IActionResult> Get(long apprenticeshipId)
        {
            var query = new GetOverlappingTrainingDateRequestQuery(apprenticeshipId);
            var result = await _mediator.Send(query);

            if (result == null) { return Ok(null); }

            var response = await _modelMapper.Map<GetOverlappingTrainingDateRequestResponce>(result);
            return Ok(response);
        }
    }
}