using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class TransferRequestWithAutoApprovalCreatedEventHandler : IHandleMessages<TransferRequestWithAutoApprovalCreatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILevyTransferMatchingApiClient _levyTransferMatchingApiClient;
        private readonly ILogger<TransferRequestWithAutoApprovalCreatedEventHandler> _logger;

        public TransferRequestWithAutoApprovalCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<TransferRequestWithAutoApprovalCreatedEventHandler> logger, ILevyTransferMatchingApiClient levyTransferMatchingApiClient)
        {
            _dbContext = dbContext;
            _logger = logger;
            _levyTransferMatchingApiClient = levyTransferMatchingApiClient;
        }

        public async Task Handle(TransferRequestWithAutoApprovalCreatedEvent message, IMessageHandlerContext context)
        {
            var db = _dbContext.Value;

            _logger.LogInformation($"Processing auto-approval for Transfer Request {message.TransferRequestId} Pledge Application {message.PledgeApplicationId}");

            var transferRequest = await db.TransferRequests.Include(c => c.Cohort)
                .SingleAsync(x => x.Id == message.TransferRequestId);

            var pledgeApplication = await _levyTransferMatchingApiClient.GetPledgeApplication(message.PledgeApplicationId);

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
