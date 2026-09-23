using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions;

public static class AlertsExtensions
{
    public static IEnumerable<Alerts> MapAlerts(this Apprenticeship source)
    {
        var result = new List<Alerts>();

        if (HasCourseDataLock(source) ||
            HasPriceDataLock(source))
        {
            result.Add(Alerts.IlrDataMismatch);
        }

        if (HasCourseDataLockPendingChanges(source) ||
            HasPriceDataLockPendingChanges(source))
        {
            result.Add(Alerts.ChangesPending);
        }

        if (HasCourseDataLockChangesRequested(source))
        {
            result.Add(Alerts.ChangesRequested);
        }
        else if (EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus(source))
        {
            result.Add(Alerts.ChangesRequested);
        }

        if (source.ApprenticeshipUpdate != null)
        {
            if (source.ApprenticeshipUpdate.Any(c =>
                    c.Originator == Originator.Employer && c.Status == ApprenticeshipUpdateStatus.Pending))
            {
                result.Add(source.IsProviderSearch ? Alerts.ChangesForReview : Alerts.ChangesPending);
            }
            else if (source.ApprenticeshipUpdate.Any(c =>
                         c.Originator == Originator.Provider && c.Status == ApprenticeshipUpdateStatus.Pending))
            {
                result.Add(source.IsProviderSearch ? Alerts.ChangesPending : Alerts.ChangesForReview);
            }
        }

        if (HasOverlappingTrainingDateRequests(source))
        {
            result.Add(Alerts.ConfirmDates);
        }

        if (HasUnacknowledgedInvalidIlrChanges(source))
        {
            result.Add(Alerts.IlrChangeInvalid);
        }

        if (HasUnacknowledgedDeclinedChanges(source))
        {
            result.Add(Alerts.ChangesDeclined);
        }

        return result;
    }

    public static bool HasUnacknowledgedInvalidIlrChanges(this Apprenticeship source)
    {
        return HasUnacknowledgedApprovalChanges(source, CocApprovalItemStatus.AutoRejected);
    }

    public static bool HasUnacknowledgedDeclinedChanges(this Apprenticeship source)
    {
        return HasUnacknowledgedApprovalChanges(source, CocApprovalItemStatus.EmployerRejected);
    }

    public static bool IsUnacknowledgedAutoRejected(this ApprovalRequest request)
    {
        return request.IsUnacknowledged(CocApprovalItemStatus.AutoRejected);
    }

    public static bool IsUnacknowledgedEmployerRejected(this ApprovalRequest request)
    {
        return request.IsUnacknowledged(CocApprovalItemStatus.EmployerRejected);
    }

    public static bool IsUnacknowledged(this ApprovalRequest request, CocApprovalItemStatus itemStatus)
    {
        return request.Status == CocApprovalResultStatus.Complete &&
               request.ProviderAcknowledgedAt == null &&
               request.Items != null &&
               request.Items.Any(item => item.Status == itemStatus);
    }

    private static bool HasUnacknowledgedApprovalChanges(Apprenticeship source, CocApprovalItemStatus itemStatus)
    {
        return source.IsProviderSearch &&
               source.ApprovalRequests != null &&
               source.ApprovalRequests.Any(request => request.IsUnacknowledged(itemStatus));
    }

    private static bool HasCourseDataLock(Apprenticeship source)
    {
        return source.DataLockStatus.Any(x =>
            x.WithCourseError() &&
            x.TriageStatus == TriageStatus.Unknown &&
            !x.IsResolved &&
            !x.IsExpired);
    }

    private static bool HasPriceDataLock(Apprenticeship source)
    {
        return source.IsProviderSearch && source.DataLockStatus.Any(x =>
            x.IsPriceOnly() &&
            x.TriageStatus == TriageStatus.Unknown &&
            !x.IsResolved &&
            !x.IsExpired);
    }

    private static bool HasCourseDataLockPendingChanges(Apprenticeship source)
    {
        return source.DataLockStatus.Any(x =>
            x.WithCourseError() &&
            x.TriageStatus == TriageStatus.Change &&
            !x.IsResolved &&
            !x.IsExpired);
    }

    private static bool HasPriceDataLockPendingChanges(Apprenticeship source)
    {
        return source.DataLockStatus.Any(x =>
            x.IsPriceOnly() &&
            x.TriageStatus == TriageStatus.Change &&
            !x.IsResolved &&
            !x.IsExpired);
    }

    private static bool HasCourseDataLockChangesRequested(Apprenticeship source)
    {
        return source.DataLockStatus.Any(x =>
            x.WithCourseError() &&
            x.TriageStatus == TriageStatus.Restart &&
            !x.IsResolved &&
            !x.IsExpired);
    }

    private static bool EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus(Apprenticeship source)
    {
        return !source.IsProviderSearch && source.DataLockStatus.Any(x =>
            x.Status == Status.Fail &&
            (x.TriageStatus != TriageStatus.Unknown && x.TriageStatus != TriageStatus.Change) &&
            !x.IsResolved &&
            !x.IsExpired);
    }

    private static bool HasOverlappingTrainingDateRequests(Apprenticeship source)
    {
        return
            !source.IsProviderSearch &&
            source.OverlappingTrainingDateRequests?.Any(x => x.Status == OverlappingTrainingDateRequestStatus.Pending) == true;
    }
}