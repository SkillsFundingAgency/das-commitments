using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
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

            if (source.ApprenticeshipUpdate == null)
            {
                return result;
            }

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

            return result;
        }

        private static bool HasCourseDataLock(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Unknown &&
                !x.IsResolved);
        }

        private static bool HasPriceDataLock(Apprenticeship source)
        {
            return source.IsProviderSearch && source.DataLockStatus.Any(x =>
                x.IsPriceOnly() &&
                x.TriageStatus == TriageStatus.Unknown &&
                !x.IsResolved);
        }

        private static bool HasCourseDataLockPendingChanges(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Change &&
                !x.IsResolved);
        }

        private static bool HasPriceDataLockPendingChanges(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.IsPriceOnly() &&
                x.TriageStatus == TriageStatus.Change &&
                !x.IsResolved);
        }

        private static bool HasCourseDataLockChangesRequested(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Restart &&
                !x.IsResolved);
        }


        private static bool EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus(Apprenticeship source)
        {
            return !source.IsProviderSearch && source.DataLockStatus.Any(x =>
                x.Status == Status.Fail &&
                (x.TriageStatus != TriageStatus.Unknown && x.TriageStatus != TriageStatus.Change) &&
                !x.IsResolved);
        }
    }
}