using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestWithAutoApprovalCreatedEventHandler : IHandleMessages<TransferRequestWithAutoApprovalCreatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IApprovalsOuterApiClient _apiClient;
        private readonly ILogger<TransferRequestWithAutoApprovalCreatedEventHandler> _logger;

        public TransferRequestWithAutoApprovalCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<TransferRequestWithAutoApprovalCreatedEventHandler> logger, IApprovalsOuterApiClient apiClient)
        {
            _dbContext = dbContext;
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task Handle(TransferRequestWithAutoApprovalCreatedEvent message, IMessageHandlerContext context)
        {
            var db = _dbContext.Value;

            _logger.LogInformation($"Processing auto-approval for Transfer Request {message.TransferRequestId} Pledge Application {message.PledgeApplicationId}");

            var transferRequest = await db.TransferRequests.Include(c => c.Cohort).ThenInclude(c => c.Apprenticeships)
                .SingleAsync(x => x.Id == message.TransferRequestId);

            if(transferRequest.Cohort.PledgeApplicationId.Value != message.PledgeApplicationId)
            {
                _logger.LogError($"Cohort PledgeApplicationId {transferRequest.Cohort.PledgeApplicationId.Value} does not match message {message.PledgeApplicationId}");
                return;
            }

            if(!transferRequest.AutoApproval)
            {
                _logger.LogError($"Transfer Request {message.TransferRequestId} is not marked for auto-approval");
                return;
            }

            var apiRequest = new GetPledgeApplicationRequest(message.PledgeApplicationId);
            var pledgeApplication = await _apiClient.Get<PledgeApplication>(apiRequest);

            if (transferRequest.FundingCap.Value <= pledgeApplication.AmountRemaining)
            {
                _logger.LogInformation($"Transfer Request Auto-Approved {message.TransferRequestId}, amount £{transferRequest.FundingCap.Value}; Pledge Application {message.PledgeApplicationId} amount remaining £{pledgeApplication.AmountRemaining}");
                transferRequest.Approve(UserInfo.System, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation($"Transfer Request Auto-Rejected {message.TransferRequestId}, amount £{transferRequest.FundingCap.Value} exceeds Pledge Application {message.PledgeApplicationId} amount remaining £{pledgeApplication.AmountRemaining}");
                transferRequest.Reject(UserInfo.System, DateTime.UtcNow);
            }
        }
    }
}
