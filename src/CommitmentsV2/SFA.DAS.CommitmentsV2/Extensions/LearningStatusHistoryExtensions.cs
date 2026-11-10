using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class LearningStatusHistoryExtensions
{
    public static Task SendStatusChangeHistory(
        this Apprenticeship apprenticeship,
        ApprenticeshipStatus previousStatus,
        DateTime appliedDate,
        LearningSourceType source,
        Func<StoreLearningHistoryCommand, Task> send,
        Guid? learningKey = null,
        UserInfo userInfo = null)
    {
        var currentStatus = apprenticeship.GetApprenticeshipStatus(appliedDate);
        if (currentStatus == previousStatus)
        {
            return Task.CompletedTask;
        }

        var previousDescription = previousStatus.GetEnumDescription().ToLowerInvariant();
        var currentDescription = currentStatus.GetEnumDescription().ToLowerInvariant();

        return send(new StoreLearningHistoryCommand
        {
            ApprenticeshipId = apprenticeship.Id,
            Source = source,
            ChangeType = source == LearningSourceType.ManualChange
                ? LearningChangeType.ManualUpdate
                : LearningChangeType.AutoApproved,
            LearningKey = learningKey,
            AppliedDate = appliedDate,
            Description = $"Status change from {previousDescription} to {currentDescription}",
            UserId = Guid.TryParse(userInfo?.UserId, out var userId) ? userId : null
        });
    }
}
