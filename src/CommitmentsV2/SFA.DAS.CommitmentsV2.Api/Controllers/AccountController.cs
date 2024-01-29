using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingApprenticeChanges;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/accounts/{accountId}")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IModelMapper _modelMapper;

        public AccountController(IMediator mediator, IModelMapper modelMapper)
        {
            _mediator = mediator;
            _modelMapper = modelMapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccount(long accountId)
        {
            var employer = await _mediator.Send(new GetAccountSummaryQuery
            {
                AccountId = accountId
            });

            return Ok(new AccountResponse
            {
                AccountId = employer.AccountId,
                LevyStatus = employer.LevyStatus
            });
        }

        [HttpGet]
        [Route("transfer-status")]
        public async Task<IActionResult> GetAccountTransferStatus(long accountId)
        {
            var status = await _mediator.Send(new GetAccountTransferStatusQuery
            {
                AccountId = accountId
            });

            return Ok(new AccountTransferStatusResponse
            {
                IsTransferReceiver = status.IsTransferReceiver,
                IsTransferSender = status.IsTransferSender
            });
        }


        [HttpGet]
        [Route("providers/approved")]
        public async Task<IActionResult> GetApprovedProviders(long accountId)
        {
            var query = new GetApprovedProvidersQuery(accountId);

            var result = await _mediator.Send(query);

            return Ok(new GetApprovedProvidersResponse(result.ProviderIds));
        }

        [HttpGet]
        [Route("provider-payments-priority")]
        public async Task<IActionResult> GetProviderPaymentsPriority(long accountId)
        {
            var result = await _mediator.Send(new GetProviderPaymentsPriorityQuery(accountId));
            var response = await _modelMapper.Map<GetProviderPaymentsPriorityResponse>(result);

            return Ok(response);
        }

        [HttpGet]
        [Route("summary")]
        public async Task<IActionResult> GetEmployerAccountSummary(long accountId)
        {
            var query = new GetApprenticeshipStatusSummaryQuery(accountId);
            var result = await _mediator.Send(query);

            if (result == null) { return NotFound(); }

            var response = await _modelMapper.Map<GetApprenticeshipStatusSummaryResponse>(result);
            return Ok(response);
        }

        [HttpGet]
        [Route("pending-apprentice-changes")]
        public async Task<IActionResult> GetPendingApprenticeChanges(long accountId)
        {
            var query = new GetPendingApprenticeChangesQuery(accountId);
            var result = await _mediator.Send(query);

            if (result == null) { return NotFound(); }

            var response = await _modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
            return Ok(response);
        }

        [HttpGet]
        [Route("ready-for-approval")]
        public async Task<IActionResult> GetEmployerCohortsReadyForApproval(long accountId)
        {
            var query = new GetEmployerCohortsReadyForApprovalQuery(accountId);
            var result = await _mediator.Send(query);

            if (result == null) { return NotFound(); }

            var response = await _modelMapper.Map<GetEmployerCohortsReadyForApprovalResponse>(result);
            return Ok(response);
        }

        [HttpPost]
        [Route("update-provider-payments-priority")]
        public async Task<IActionResult> UpdateProviderPaymentsPriority(long accountId, [FromBody] UpdateProviderPaymentsPriorityRequest request)
        {
            await _mediator.Send(new UpdateProviderPaymentsPriorityCommand(
                accountId,
                request.ProviderPriorities.Select(p => new UpdateProviderPaymentsPriorityCommand.ProviderPaymentPriorityUpdateItem
                {
                    ProviderId = p.ProviderId,
                    PriorityOrder = p.PriorityOrder
                }).ToList(),
                request.UserInfo));

            return Ok();
        }
    }
}