using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using ApprenticeshipEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfers(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<ApprenticeshipEmployerTypeChangeEventHandlerToAutoRejectTransfers> logger)
    : IHandleMessages<ApprenticeshipEmployerTypeChangeEvent>
{
    public async Task Handle(ApprenticeshipEmployerTypeChangeEvent message, IMessageHandlerContext context)
    {
        if (message.ApprenticeshipEmployerType != ApprenticeshipEmployerType.NonLevy)
        {
            logger.LogInformation(
                "Ignoring ApprenticeshipEmployerTypeChangeEvent for account {AccountId} because employer type is {EmployerType}",
                message.AccountId,
                message.ApprenticeshipEmployerType);
            return;
        }

        logger.LogInformation("Handling ApprenticeshipEmployerTypeChangeEvent for account {AccountId}", message.AccountId);

        var transferRequestIds = await dbContext.Value.TransferRequests
            .Where(tr => tr.Status == TransferApprovalStatus.Pending
                         && tr.Cohort.TransferSenderId == message.AccountId)
            .Select(tr => tr.Id)
            .ToListAsync();

        var rejectedOn = DateTime.UtcNow;

        foreach (var transferRequestId in transferRequestIds)
        {
            await context.Send(
                new RejectTransferRequestCommand(transferRequestId, rejectedOn, UserInfo.System),
                new SendOptions());
        }

        logger.LogInformation(
            "Queued {TransferRequestCount} transfer request rejections for account {AccountId}",
            transferRequestIds.Count,
            message.AccountId);
    }
}
