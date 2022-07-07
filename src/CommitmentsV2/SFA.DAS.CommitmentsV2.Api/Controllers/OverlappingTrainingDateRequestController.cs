using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;
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
        public async Task<IActionResult> CreateOverlappingTrainingDate(long providerId, [FromBody]CreateOverlappingTrainingDateRequest request)
        {
            var result = await _mediator.Send(new CreateOverlappingTrainingDateRequestCommand
            {
                ProviderId = providerId,
                ApprneticeshipId = request.DraftApprenticeshipId,
                PreviousApprenticeshipId = request.PreviousApprenticeshipId,
                UserInfo = request.UserInfo
            });

            return Ok( new CreateOverlappingTrainingDateResponse { Id = result.Id} );
        }
    }
}
