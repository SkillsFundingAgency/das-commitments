using System;
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
            else if(EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus(source))
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

        public static IQueryable<Apprenticeship> FilterApprenticeshipByAlert(this IQueryable<Apprenticeship> apprenticeships, Alerts alert)
        {
            return alert switch
            {
                Alerts.IlrDataMismatch => apprenticeships.Where(a =>
                    (
                              a.DataLockStatus.Any(x =>
                               ( x.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                            || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                            || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                            || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)) &&
                            x.TriageStatus == TriageStatus.Unknown &&
                            !x.IsResolved)
                      ) ||
                    (
                        a.DataLockStatus.Any(x =>
                           ((int)x.ErrorCode == (int)DataLockErrorCode.Dlock07 ) &&
                            x.TriageStatus == TriageStatus.Unknown &&
                            !x.IsResolved)
                    )
                ),
                

                Alerts.ChangesPending => apprenticeships.Where(a => HasCourseDataLockPendingChanges(a) || HasPriceDataLockPendingChanges(a)),
                Alerts.ChangesRequested => apprenticeships.Where(a => HasCourseDataLockChangesRequested(a) || EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus(a)),
                Alerts.ChangesForReview => apprenticeships.Where(a => HasChangesForReview(a)),
                _ => throw new NotImplementedException($"Alert {alert} not implemented")
            };
        }

        private static IQueryable<Apprenticeship> HasCourseDataLock(IQueryable<Apprenticeship> apprenticeships)
        {
            return apprenticeships.Where(o => o.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Unknown &&
                !x.IsResolved));
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


        private static bool HasChangesForReview(Apprenticeship source)
        {
            if (source.ApprenticeshipUpdate == null)
                return false;
            
            if (source.ApprenticeshipUpdate.Any(c =>
                c.Originator == Originator.Employer && c.Status == ApprenticeshipUpdateStatus.Pending))
            {
                if (source.IsProviderSearch)
                    return true;
            }
            else if (source.ApprenticeshipUpdate.Any(c =>
                c.Originator == Originator.Provider && c.Status == ApprenticeshipUpdateStatus.Pending))
            {
                if (!source.IsProviderSearch)
                    return true;
            }

            return false;
        }

    }
}