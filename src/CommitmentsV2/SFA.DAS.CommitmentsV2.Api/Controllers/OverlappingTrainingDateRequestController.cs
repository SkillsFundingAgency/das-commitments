using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/overlapping-training-date-request")]
    public class OverlappingTrainingDateRequestController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OverlappingTrainingDateRequestController(IMediator mediator)
        {
            _mediator = mediator;
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
        public async Task<IActionResult> ValidateDraftAppretniceship(long providerId, [FromBody] ValidateDraftApprenticeshipRequest request)
        {
            var command = new ValidateDraftApprenticeshipDetailsCommand { DraftApprenticeshipRequest = request };
            await _mediator.Send(command);
            return Ok();
        }
    }
}
