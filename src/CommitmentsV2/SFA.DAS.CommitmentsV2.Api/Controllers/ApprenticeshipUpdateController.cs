using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.RejectApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.UndoApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/apprenticeships/{ApprenticeshipId}/updates")]
    public class ApprenticeshipUpdateController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public ApprenticeshipUpdateController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetApprenticeshipUpdates(long apprenticeshipId,
            [FromQuery] GetApprenticeshipUpdatesRequest request)
        {
            var result = await _mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId, request.Status));
            var response = await _modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
            return Ok(response);
        }

        [HttpPost]
        [Route("accept-apprenticeship-update")]
        public async Task<IActionResult> AcceptApprenticeshipUpdates(long apprenticeshipId,
            [FromBody] AcceptApprenticeshipUpdatesRequest request)
        {
            await _mediator.Send(new AcceptApprenticeshipUpdatesCommand
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = request.UserInfo,
                AccountId = request.AccountId
            });

            return Ok();
        }


        [HttpPost]
        [Route("reject-apprenticeship-update")]
        public async Task<IActionResult> RejectApprenticeshipUpdates(long apprenticeshipId,
            [FromBody] RejectApprenticeshipUpdatesRequest request)
        {
            await _mediator.Send(new RejectApprenticeshipUpdatesCommand
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = request.UserInfo,
                AccountId = request.AccountId
            });

            return Ok();
        }

        [HttpPost]
        [Route("undo-apprenticeship-update")]
        public async Task<IActionResult> UndoApprenticeshipUpdates(long apprenticeshipId,
            [FromBody] UndoApprenticeshipUpdatesRequest request)
        {
            await _mediator.Send(new UndoApprenticeshipUpdatesCommand
            {
                ApprenticeshipId = apprenticeshipId,
                UserInfo = request.UserInfo,
                AccountId = request.AccountId
            });

            return Ok();
        }
    }
}