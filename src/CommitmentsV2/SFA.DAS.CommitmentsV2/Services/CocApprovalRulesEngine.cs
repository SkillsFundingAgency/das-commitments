using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Services;

public class CocApprovalRulesEngine(
    ICocApprovalStatusService cocApprovalService,
    ILogger<CocApprovalRulesEngine> logger,
    INotifyProviderService notifyProviderService,
    IMessageSession messageSession,
    CommitmentsV2Configuration commitmentsV2Configuration,
    IEncodingService encodingService) : ICocApprovalRulesEngine
{
    private const string ProviderRequestRejectedNotificationEmailTemplate = "ProviderRequestRejectedNotification";

    public async Task<CocApprovalState> DetermineApprovalState(CocApprovalDetails cocApprovalDetails)
    {
        logger.LogInformation("Determining Approval State");
        var updateStatuses = cocApprovalService.DetermineCocUpdateStatuses(cocApprovalDetails.Updates, cocApprovalDetails.Apprenticeship);
        var approvalRequestStatus = DetermineApprovalRequestStatus(updateStatuses);
        IEnumerable<ApprovalFieldRequest> approvalFieldRequests = MapToApprovalFieldRequests(cocApprovalDetails, updateStatuses);

        if (updateStatuses.Any(i => i.Status == CocApprovalItemStatus.AutoRejected))
        {         
            var apprenticeship = encodingService.Encode(cocApprovalDetails.ApprenticeshipId, EncodingType.ApprenticeshipId);
            await notifyProviderService.NotifyProvider(cocApprovalDetails.ProviderId, apprenticeship, ProviderRequestRejectedNotificationEmailTemplate);
        }

        if (updateStatuses.Any(i => i.Status == CocApprovalItemStatus.AutoApproved))
        {
            await SendEmployerAutoApprovalNotification(cocApprovalDetails.Apprenticeship);
        }

        return new CocApprovalState
        {
            ApprovalRequest = new ApprovalRequest
            {
                LearningKey = cocApprovalDetails.LearningKey,
                ApprenticeshipId = cocApprovalDetails.ApprenticeshipId,
                LearningType = cocApprovalDetails.LearningType,
                UKPRN = cocApprovalDetails.ProviderId.ToString(),
                ULN = cocApprovalDetails.ULN,
                Status = approvalRequestStatus,
                Items = approvalFieldRequests.ToList()
            },
            ApprovalResult = new CocApprovalResult
            {
                Status = approvalRequestStatus,
                Items = updateStatuses
            }
        };
    }

    private static IEnumerable<ApprovalFieldRequest> MapToApprovalFieldRequests(CocApprovalDetails command, List<CocUpdateResult> updateStatuses)
    {
        return command.ApprovalFieldChanges.Join(
            updateStatuses,
            change => change.ChangeType,
            status => status.Field.GetEnumDescription(),
            (change, status) => new ApprovalFieldRequest
            {
                Field = change.ChangeType,
                Old = change.Data.Old,
                New = change.Data.New,
                EffectiveFromDate = change.Data.EffectiveFromDate,
                Status = status.Status,
                Reason = status.Reason
            }
            );
    }

    private static CocApprovalResultStatus DetermineApprovalRequestStatus(List<CocUpdateResult> updateResults)
    {
        if (updateResults.Any(x => x.Status == CocApprovalItemStatus.Pending))
            return CocApprovalResultStatus.Pending;

        return CocApprovalResultStatus.Complete;
    }

    private async Task SendEmployerAutoApprovalNotification(Apprenticeship apprenticeship)
    {
        var accountId = apprenticeship.Cohort.EmployerAccountId;
        var encodedApprenticeshipId = encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId);
        var encodedAccountId = encodingService.Encode(accountId, EncodingType.AccountId);

        var sendEmailCommand = new SendEmailToEmployerCommand(
            accountId,
            "EmployerAutoApprovedNotification",
            new Dictionary<string, string>
            {
                {"provider_name", apprenticeship.Cohort.Provider.Name},
                {
                    "link_to_manage_apprenticeships",
                    $"{commitmentsV2Configuration.EmployerCommitmentsBaseUrl}{encodedAccountId}/apprentices/{encodedApprenticeshipId}/details"
                }
            },
            null,
            "Name"
            );

        logger.LogInformation("Sending EmployerAutoApprovedNotification Email for id {0} hashed as {1}", apprenticeship.Id, encodedApprenticeshipId);
        await messageSession.Send(sendEmailCommand);
    }
}

public class CocApprovalState
{
    public ApprovalRequest ApprovalRequest { get; set; }
    public CocApprovalResult ApprovalResult { get; set; }
}