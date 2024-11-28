using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateTransferApprovalForSender;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/accounts")]
[ApiController]
public class TransferRequestController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpGet]
    [Route("{transferSenderId:long}/sender/transfers/{transferRequestId:long}", Name = "GetTransferRequestForSender")]
    public async Task<IActionResult> GetTransferRequestForSender(long transferSenderId, long transferRequestId)
    {
        var result = await mediator.Send(new GetTransferRequestQuery(transferSenderId, transferRequestId, GetTransferRequestQuery.QueryOriginator.TransferSender));
        if (result == null)
        {
            return NotFound();
        }

        var response = await modelMapper.Map<GetTransferRequestResponse>(result);
        return Ok(response);
    }

    [HttpPost]
    [Route("{transferSenderId:long}/transfers/{transferRequestId:long}/approval/{cohortId:long}", Name = "UpdateTransferApprovalForSender")]
    public async Task<IActionResult> UpdateTransferApprovalForSender(long transferSenderId, long transferRequestId, long cohortId, UpdateTransferApprovalForSenderRequest request)
    {
        await mediator.Send(new UpdateTransferApprovalForSenderCommand(
            transferSenderId,
            request.TransferReceiverId,
            transferRequestId,
            cohortId,
            request.TransferApprovalStatus,
            request.UserInfo));

        return Ok();
    }

    [HttpGet]
    [Route("{transferReceiverId:long}/receiver/transfers/{transferRequestId:long}", Name = "GetTransferRequestForReceiver")]
    public async Task<IActionResult> GetTransferRequestForReceiver(long transferReceiverId, long transferRequestId)
    {
        var result = await mediator.Send(new GetTransferRequestQuery(transferReceiverId, transferRequestId, GetTransferRequestQuery.QueryOriginator.TransferReceiver));
        if (result == null)
        {
            return NotFound();
        }

        var response = await modelMapper.Map<GetTransferRequestResponse>(result);
        
        return Ok(response);
    }

    [HttpGet]
    [Route("{accountId:long}/transfers", Name = "GetTransferRequests")]
    public async Task<IActionResult> GetTransferRequests(long accountId, [FromQuery] TransferType? originator = null)
    {
        var result = await mediator.Send(new GetTransferRequestsSummaryQuery(accountId, originator));
        if (result == null)
        {
            return NotFound();
        }

        var response = await modelMapper.Map<GetTransferRequestSummaryResponse>(result);

        return Ok(response);
    }
}