using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class TransferRequestWithAutoApprovalCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<TransferRequestWithAutoApprovalCreatedEventHandler> logger, IApprovalsOuterApiClient apiClient)
    : IHandleMessages<TransferRequestWithAutoApprovalCreatedEvent>
{
    public async Task Handle(TransferRequestWithAutoApprovalCreatedEvent message, IMessageHandlerContext context)
    {
        var db = dbContext.Value;

        logger.LogInformation("Processing auto-approval for Transfer Request {TransferRequestId} Pledge Application {PledgeApplicationId}", message.TransferRequestId, message.PledgeApplicationId);

        var transferRequest = await db.TransferRequests.Include(c => c.Cohort).ThenInclude(c => c.Apprenticeships)
            .SingleAsync(x => x.Id == message.TransferRequestId);

        if (transferRequest.Cohort.PledgeApplicationId.Value != message.PledgeApplicationId)
        {
            logger.LogError("Cohort PledgeApplicationId {CohortPledgeApplicationId} does not match message {PledgeApplicationId}", transferRequest.Cohort.PledgeApplicationId.Value, message.PledgeApplicationId);
            return;
        }

        if (!transferRequest.AutoApproval)
        {
            logger.LogError("Transfer Request {TransferRequestId} is not marked for auto-approval", message.TransferRequestId);
            return;
        }

        var apiRequest = new GetPledgeApplicationRequest(message.PledgeApplicationId);
        var pledgeApplication = await apiClient.Get<PledgeApplication>(apiRequest);

        if (transferRequest.FundingCap.Value <= pledgeApplication.AmountRemaining)
        {
            logger.LogInformation("Transfer Request Auto-Approved {TransferRequestId}, amount £{FundingCapValue}; Pledge Application {PledgeApplicationId} amount remaining £{AmountRemaining}",
                message.TransferRequestId,
                transferRequest.FundingCap.Value,
                message.PledgeApplicationId,
                pledgeApplication.AmountRemaining
            );

            transferRequest.Approve(UserInfo.System, DateTime.UtcNow);
        }
        else
        {
            logger.LogInformation("Transfer Request Auto-Rejected {TransferRequestId}, amount £{FundingCapValue}; Pledge Application {PledgeApplicationId} amount remaining £{AmountRemaining}",
                message.TransferRequestId,
                transferRequest.FundingCap.Value,
                message.PledgeApplicationId,
                pledgeApplication.AmountRemaining
            );
            
            transferRequest.Reject(UserInfo.System, DateTime.UtcNow);
        }
    }
}