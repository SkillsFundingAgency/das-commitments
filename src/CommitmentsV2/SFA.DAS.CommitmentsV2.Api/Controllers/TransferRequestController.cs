using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateTransferApprovalForSender;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequestsSummary;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class TransferRequestController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public TransferRequestController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        [Route("{transferSenderId}/sender/transfers/{transferRequestId}", Name = "GetTransferRequestForSender")]
        public async Task<IActionResult> GetTransferRequestForSender(long transferSenderId, long transferRequestId)
        {
            var result = await _mediator.Send(new GetTransferRequestQuery(transferSenderId, transferRequestId, GetTransferRequestQuery.QueryOriginator.TransferSender));
            if (result == null)
            {
                return NotFound();
            }

            var response = await _modelMapper.Map<GetTransferRequestResponse>(result);
            return Ok(response);
        }

        [HttpPost]
        [Route("{transferSenderId}/transfers/{transferRequestId}/approval/{cohortId}", Name = "UpdateTransferApprovalForSender")]
        public async Task<IActionResult> UpdateTransferApprovalForSender(long transferSenderId, long transferRequestId, long cohortId, UpdateTransferApprovalForSenderRequest request)
        {
            await _mediator.Send(new UpdateTransferApprovalForSenderCommand(
                transferSenderId,
                request.TransferReceiverId,
                transferRequestId,
                cohortId,
                request.TransferApprovalStatus,
                request.UserInfo));

            return Ok();
        }

        [HttpGet]
        [Route("{transferReceiverId}/receiver/transfers/{transferRequestId}", Name = "GetTransferRequestForReceiver")]
        public async Task<IActionResult> GetTransferRequestForReceiver(long transferReceiverId, long transferRequestId)
        {
            var result = await _mediator.Send(new GetTransferRequestQuery(transferReceiverId, transferRequestId, GetTransferRequestQuery.QueryOriginator.TransferReceiver));
            if (result == null)
            {
                return NotFound();
            }

            var response = await _modelMapper.Map<GetTransferRequestResponse>(result);
            return Ok(response);
        }

        [HttpGet]
        [Route("{accountId}/transfers", Name = "GetTransferRequests")]
        public async Task<IActionResult> GetTransferRequests(long accountId, [FromQuery] TransferType? originator = null)
        {
            var result = await _mediator.Send(new GetTransferRequestsSummaryQuery(accountId, originator));
            if (result == null)
            {
                return NotFound();
            }

            var response = await _modelMapper.Map<GetTransferRequestSummaryResponse>(result);

            return Ok(response);
        }
    }
}
