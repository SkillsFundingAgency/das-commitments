using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.RejectApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.UndoApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/apprenticeships/{ApprenticeshipId:long}/updates")]
public class ApprenticeshipUpdateController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetApprenticeshipUpdates(long apprenticeshipId, [FromQuery] GetApprenticeshipUpdatesRequest request)
    {
        var result = await mediator.Send(new GetApprenticeshipUpdateQuery(apprenticeshipId, request.Status));
        var response = await modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
        return Ok(response);
    }

    [HttpPost]
    [Route("accept-apprenticeship-update")]
    public async Task<IActionResult> AcceptApprenticeshipUpdates(long apprenticeshipId, [FromBody] AcceptApprenticeshipUpdatesRequest request)
    {
        await mediator.Send(new AcceptApprenticeshipUpdatesCommand
        {
            ApprenticeshipId = apprenticeshipId,
            UserInfo = request.UserInfo,
            AccountId = request.AccountId
        });

        return Ok();
    }


    [HttpPost]
    [Route("reject-apprenticeship-update")]
    public async Task<IActionResult> RejectApprenticeshipUpdates(long apprenticeshipId, [FromBody] RejectApprenticeshipUpdatesRequest request)
    {
        await mediator.Send(new RejectApprenticeshipUpdatesCommand
        {
            ApprenticeshipId = apprenticeshipId,
            UserInfo = request.UserInfo,
            AccountId = request.AccountId
        });

        return Ok();
    }

    [HttpPost]
    [Route("undo-apprenticeship-update")]
    public async Task<IActionResult> UndoApprenticeshipUpdates(long apprenticeshipId, [FromBody] UndoApprenticeshipUpdatesRequest request)
    {
        await mediator.Send(new UndoApprenticeshipUpdatesCommand
        {
            ApprenticeshipId = apprenticeshipId,
            UserInfo = request.UserInfo,
            AccountId = request.AccountId
        });

        return Ok();
    }
}