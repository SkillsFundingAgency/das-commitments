using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/accounts")]
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
        [Route("{AccountId}")]
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
        [Route("{AccountId}/providers/approved")]
        public async Task<IActionResult> GetApprovedProviders(long accountId)
        {
            var query = new GetApprovedProvidersQuery(accountId);

            var result = await _mediator.Send(query);

            return Ok(new GetApprovedProvidersResponse(result.ProviderIds));
        }

        [HttpGet]
        [Route("{accountId}/provider-payments-priority")]
        public async Task<IActionResult> GetProviderPaymentsPriority(long accountId)
        {
            var result = await _mediator.Send(new GetProviderPaymentsPriorityQuery(accountId));
            var response = await _modelMapper.Map<GetProviderPaymentsPriorityResponse>(result);

            return Ok(response);
        }

        [HttpPost]
        [Route("{accountId}/update-provider-payments-priority")]
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