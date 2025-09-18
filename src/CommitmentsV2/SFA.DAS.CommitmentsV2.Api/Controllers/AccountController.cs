using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountTransferStatus;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPendingApprenticeChanges;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/accounts/{accountId:long}")]
[ApiController]
[Authorize]
public class AccountController(IMediator mediator, IModelMapper modelMapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAccount(long accountId)
    {
        var employer = await mediator.Send(new GetAccountSummaryQuery
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
        var status = await mediator.Send(new GetAccountTransferStatusQuery
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
    [Route("status")]
    public async Task<IActionResult> GetAccountStatus(long accountId, int completionLag, int startLag, int newStartWindow)
    {
        var accountStatus = await mediator.Send(new GetAccountStatusQuery
        {
            AccountId = accountId,
            CompletionLag = completionLag,
            StartLag = startLag,
            NewStartWindow = newStartWindow
        });

        return Ok(new AccountStatusResponse
        {
            Active = accountStatus.Active,
            Completed = accountStatus.Completed,
            NewStart = accountStatus.NewStart
        });
    }

    [HttpGet]
    [Route("providers/approved")]
    public async Task<IActionResult> GetApprovedProviders(long accountId)
    {
        var query = new GetApprovedProvidersQuery(accountId);

        var result = await mediator.Send(query);

        return Ok(new GetApprovedProvidersResponse(result.ProviderIds));
    }

    [HttpGet]
    [Route("provider-payments-priority")]
    public async Task<IActionResult> GetProviderPaymentsPriority(long accountId)
    {
        var result = await mediator.Send(new GetProviderPaymentsPriorityQuery(accountId));
        var response = await modelMapper.Map<GetProviderPaymentsPriorityResponse>(result);

        return Ok(response);
    }

    [HttpGet]
    [Route("summary")]
    public async Task<IActionResult> GetEmployerAccountSummary(long accountId)
    {
        var query = new GetApprenticeshipStatusSummaryQuery(accountId);
        var result = await mediator.Send(query);

        if (result == null) { return NotFound(); }

        var response = await modelMapper.Map<GetApprenticeshipStatusSummaryResponse>(result);
        return Ok(response);
    }

    [HttpGet]
    [Route("pending-apprentice-changes")]
    public async Task<IActionResult> GetPendingApprenticeChanges(long accountId)
    {
        var query = new GetPendingApprenticeChangesQuery(accountId);
        var result = await mediator.Send(query);

        if (result == null) { return NotFound(); }

        var response = await modelMapper.Map<GetApprenticeshipUpdatesResponse>(result);
        return Ok(response);
    }

    [HttpPost]
    [Route("update-provider-payments-priority")]
    public async Task<IActionResult> UpdateProviderPaymentsPriority(long accountId, [FromBody] UpdateProviderPaymentsPriorityRequest request)
    {
        await mediator.Send(new UpdateProviderPaymentsPriorityCommand(
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