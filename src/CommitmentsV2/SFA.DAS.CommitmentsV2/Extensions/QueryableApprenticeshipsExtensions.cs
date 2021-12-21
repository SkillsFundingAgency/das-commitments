using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class QueryableApprenticeshipsExtensions
    {
        public static IQueryable<Apprenticeship> Filter(this IQueryable<Apprenticeship> apprenticeships, ApprenticeshipSearchFilters filters, bool isProvider = true)
        {
            if (filters == null)
            {
                return apprenticeships;
            }

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                if (long.TryParse(filters.SearchTerm, out var result))
                {
                    apprenticeships = apprenticeships.Where(app =>
                        app.Uln == filters.SearchTerm);
                }
                else
                {
                    var found = new List<long>();

                    if (!filters.SearchTerm.Contains(" "))
                    {
                        found.AddRange(apprenticeships.Where(app =>
                                app.FirstName.StartsWith(filters.SearchTerm))
                            .Select(apprenticeship => apprenticeship.Id));

                        found.AddRange(apprenticeships.Where(app =>
                                app.LastName.StartsWith(filters.SearchTerm))
                            .Select(apprenticeship => apprenticeship.Id));
                    }
                    else
                    {
                        var firstName = filters.SearchTerm.Substring(0, filters.SearchTerm.IndexOf(' '));
                        var lastName = filters.SearchTerm.Substring(firstName.Length + 1);

                        found.AddRange(apprenticeships.Where(app =>
                                app.FirstName.StartsWith(firstName) &&
                                app.LastName.StartsWith(lastName))
                            .Select(apprenticeship => apprenticeship.Id));
                    }

                    apprenticeships = apprenticeships.Where(apprenticeship =>
                        found.Contains(apprenticeship.Id));
                }
            }

            if (!string.IsNullOrEmpty(filters.EmployerName))
            {
                apprenticeships = apprenticeships.Where(app => app.Cohort != null && filters.EmployerName.Equals(app.Cohort.AccountLegalEntity.Name));
            }

            if (!string.IsNullOrEmpty(filters.ProviderName))
            {
                apprenticeships = apprenticeships.Where(app => app.Cohort != null && filters.ProviderName.Equals(app.Cohort.Provider.Name));
            }

            if (!string.IsNullOrEmpty(filters.CourseName))
            {
                apprenticeships = apprenticeships.Where(app => filters.CourseName.Equals(app.CourseName));
            }

            if (filters.Status.HasValue)
            {
                var paymentStatuses = filters.Status.Value.MapToPaymentStatus();

                apprenticeships = apprenticeships.Where(app => paymentStatuses == app.PaymentStatus);
                switch (filters.Status)
                {
                    case ApprenticeshipStatus.WaitingToStart:
                        apprenticeships = apprenticeships.Where(c => c.StartDate.HasValue && c.StartDate >= DateTime.UtcNow);
                        break;
                    case ApprenticeshipStatus.Live:
                        apprenticeships = apprenticeships.Where(c => c.StartDate.HasValue && c.StartDate <= DateTime.UtcNow);
                        break;
                }
            }

            if (filters.StartDate.HasValue)
            {
                apprenticeships = apprenticeships.Where(app =>
                    app.StartDate.HasValue &&
                    filters.StartDate.Value.Month.Equals(app.StartDate.Value.Month) &&
                    filters.StartDate.Value.Year.Equals(app.StartDate.Value.Year));
            }

            if (filters.EndDate.HasValue)
            {
                apprenticeships = apprenticeships.Where(app =>
                    app.EndDate.HasValue &&
                    filters.EndDate.Value.Month.Equals(app.EndDate.Value.Month) &&
                    filters.EndDate.Value.Year.Equals(app.EndDate.Value.Year));
            }

            if (filters.AccountLegalEntityId.HasValue)
            {
                apprenticeships = apprenticeships.Where(x => x.Cohort != null && x.Cohort.AccountLegalEntityId == filters.AccountLegalEntityId.Value);
            }

            if (filters.StartDateRange != null)
            {
                if (filters.StartDateRange.From.HasValue)
                {
                    apprenticeships = apprenticeships.Where(x => x.StartDate.HasValue && x.StartDate.Value >= filters.StartDateRange.From);
                }

                if (filters.StartDateRange.To.HasValue)
                {
                    apprenticeships = apprenticeships.Where(x => x.StartDate.HasValue && x.StartDate.Value <= filters.StartDateRange.To);
                }
            }

            if (filters.Alert.HasValue)
            {
                apprenticeships = FilterApprenticeshipByAlert(apprenticeships, filters.Alert.Value, isProvider);
            }

            if (filters.ApprenticeConfirmationStatus.HasValue)
            {
                switch (filters.ApprenticeConfirmationStatus)
                {
                    case ConfirmationStatus.Confirmed:
                        apprenticeships = apprenticeships.Where(x => x.ApprenticeshipConfirmationStatus.ApprenticeshipConfirmedOn.HasValue);
                        break;

                    case ConfirmationStatus.Unconfirmed:
                        apprenticeships = apprenticeships.Where(x => x.Email != null &&
                                                                        x.ApprenticeshipConfirmationStatus.ApprenticeshipConfirmedOn == null &&
                                                                        (x.ApprenticeshipConfirmationStatus.ConfirmationOverdueOn == null ||
                                                                        DateTime.UtcNow < x.ApprenticeshipConfirmationStatus.ConfirmationOverdueOn));
                        break;

                    case ConfirmationStatus.Overdue:
                        apprenticeships = apprenticeships.Where(x => x.Email != null &&
                                                                        x.ApprenticeshipConfirmationStatus.ApprenticeshipConfirmedOn == null &&
                                                                        DateTime.UtcNow > x.ApprenticeshipConfirmationStatus.ConfirmationOverdueOn);
                        break;

                    case ConfirmationStatus.NA:
                        apprenticeships = apprenticeships.Where(x => x.Email == null);
                        break;
                }
            }

            return apprenticeships;
        }

        public static IQueryable<Apprenticeship> WithAlerts(
            this IQueryable<Apprenticeship> apprenticeships, bool hasAlerts,
            IEmployerProviderIdentifier identifier)
        {
            return identifier.ProviderId.HasValue ? WithAlertsProvider(apprenticeships, hasAlerts) : WithAlertsEmployer(apprenticeships, hasAlerts);
        }

        private static IQueryable<Apprenticeship> WithAlertsProvider(this IQueryable<Apprenticeship> apprenticeships, bool hasAlerts)
        {
            if (hasAlerts)
            {
                return apprenticeships.Where(apprenticeship => apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != EventStatus.Removed && !c.IsExpired) ||
                                                                   apprenticeship.ApprenticeshipUpdate != null &&
                                                                   apprenticeship.ApprenticeshipUpdate.Any(
                                                                       c => c.Status == ApprenticeshipUpdateStatus.Pending
                                                                            && (c.Originator == Originator.Employer
                                                                                || c.Originator == Originator.Provider)));
            }

            return apprenticeships.Where(apprenticeship =>
                !apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != EventStatus.Removed && !c.IsExpired) &&
                (apprenticeship.ApprenticeshipUpdate.Any() || apprenticeship.ApprenticeshipUpdate.All(c => c.Status != ApprenticeshipUpdateStatus.Pending)));
        }
        private static IQueryable<Apprenticeship> WithAlertsEmployer(this IQueryable<Apprenticeship> apprenticeships, bool hasAlerts)
        {
            if (hasAlerts)
            {
                return apprenticeships.Where(apprenticeship => apprenticeship.DataLockStatus.Any(c => !c.IsResolved
                                                                                                      && c.Status == Status.Fail
                                                                                                      && c.EventStatus != EventStatus.Removed
                                                                                                      && c.TriageStatus != TriageStatus.Unknown
                                                                                                      && !c.IsExpired) ||
                                                               apprenticeship.ApprenticeshipUpdate != null &&
                                                               apprenticeship.ApprenticeshipUpdate.Any(
                                                                   c => c.Status == ApprenticeshipUpdateStatus.Pending
                                                                        && (c.Originator == Originator.Employer
                                                                            || c.Originator == Originator.Provider)));
            }
            return apprenticeships.Where(apprenticeship =>
                !apprenticeship.DataLockStatus.Any(c => !c.IsResolved 
                                                        && c.Status == Status.Fail 
                                                        && c.EventStatus != EventStatus.Removed
                                                        && c.TriageStatus != TriageStatus.Unknown
                                                        && !c.IsExpired) &&

                (apprenticeship.ApprenticeshipUpdate == null ||
                 apprenticeship.ApprenticeshipUpdate.All(c => c.Status != ApprenticeshipUpdateStatus.Pending)));
        }

        public static IQueryable<Apprenticeship> WithProviderOrEmployerId(this IQueryable<Apprenticeship> apprenticeships, IEmployerProviderIdentifier identifier)
        {
            if (identifier.ProviderId.HasValue)
            {
                return apprenticeships
                        .Include(app => app.ApprenticeshipConfirmationStatus)
                        .Where(app => app.Cohort.ProviderId == identifier.ProviderId);
            }

            return identifier.EmployerAccountId.HasValue ?
                apprenticeships
                    .Include(app => app.ApprenticeshipConfirmationStatus)
                    .Include(app => app.Cohort)
                    .Where(app => app.Cohort.EmployerAccountId == identifier.EmployerAccountId)
                     : apprenticeships;
        }

        public static IQueryable<Apprenticeship> FilterApprenticeshipByAlert(IQueryable<Apprenticeship> apprenticeships, Alerts alert, bool isProvider)
        {
            // Doing this because we can't use extension method for LINQ to SQL query
            switch (alert)
            {
                case Alerts.IlrDataMismatch:
                    return FilterApprenticeshipByAlertForIlrDataMismatch(apprenticeships, alert);
                case Alerts.ChangesPending:
                    return FilterApprenticeshipByAlertForChangesPending(apprenticeships, alert, isProvider);
                case Alerts.ChangesRequested:
                    return FilterApprenticeshipByAlertForChangesRequested(apprenticeships, alert, isProvider);
                case Alerts.ChangesForReview:
                    return FilterApprenticeshipByAlertForChangesForReview(apprenticeships, alert, isProvider);
                default:
                    throw new ArgumentOutOfRangeException(nameof(alert), alert, null);
            }

        }

        public static IQueryable<Apprenticeship> FilterApprenticeshipByAlertForIlrDataMismatch(IQueryable<Apprenticeship> apprenticeships, Alerts alert)
        {
            return apprenticeships.Where(a =>
                                  (
                                      //HasCourseDataLock
                                      a.DataLockStatus.Any(x =>
                                          (x.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                                           || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                                           || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                                           || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)) &&
                                          x.TriageStatus == TriageStatus.Unknown &&
                                          !x.IsResolved && !x.IsExpired)
                                  ) ||
                                  (
                                      //Has Only PriceDataLock
                                      a.DataLockStatus.Any(x =>
                                          ((int)x.ErrorCode == (int)DataLockErrorCode.Dlock07) &&
                                          x.TriageStatus == TriageStatus.Unknown &&
                                          !x.IsResolved && !x.IsExpired)
                                  )
                              );
        }

        public static IQueryable<Apprenticeship> FilterApprenticeshipByAlertForChangesPending(IQueryable<Apprenticeship> apprenticeships, Alerts alert, bool isProvider)
        {
            if (isProvider)
            {
                return apprenticeships.Where(a =>
                     (
                        //HasCourseDataLockPendingChanges
                        a.DataLockStatus.Any(x =>
                            (
                                x.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                                || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                                || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                                || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)
                            ) &&
                            x.TriageStatus == TriageStatus.Change &&
                            !x.IsResolved && !x.IsExpired)
                    )
                    ||
                    (
                        //HasPrice Only DataLockPendingChanges
                        a.DataLockStatus.Any(x =>
                            (
                                (int)x.ErrorCode == (int)DataLockErrorCode.Dlock07
                            ) &&
                            x.TriageStatus == TriageStatus.Change &&
                            !x.IsResolved && !x.IsExpired)
                    )
                    ||
                    (

                        a.ApprenticeshipUpdate != null &&
                        a.ApprenticeshipUpdate.Any(c => c.Originator == Originator.Provider && c.Status == ApprenticeshipUpdateStatus.Pending)

                    ));
            }
            else
            {

                return apprenticeships.Where(a =>
                         (
                            //HasCourseDataLockPendingChanges
                            a.DataLockStatus.Any(x =>
                                (
                                    x.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                                    || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                                    || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                                    || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)
                                ) &&
                                x.TriageStatus == TriageStatus.Change &&
                                !x.IsResolved && !x.IsExpired)
                        )
                        ||
                        (
                            //HasPrice Only DataLockPendingChanges
                            a.DataLockStatus.Any(x =>
                                (
                                    (int)x.ErrorCode == (int)DataLockErrorCode.Dlock07
                                ) &&
                                x.TriageStatus == TriageStatus.Change &&
                                !x.IsResolved && !x.IsExpired)
                        )
                        ||
                        (
                            a.ApprenticeshipUpdate != null &&
                            a.ApprenticeshipUpdate.Any(c => c.Originator == Originator.Employer && c.Status == ApprenticeshipUpdateStatus.Pending)

                        ));
            }
        }

        public static IQueryable<Apprenticeship> FilterApprenticeshipByAlertForChangesRequested(IQueryable<Apprenticeship> apprenticeships, Alerts alert, bool isProvider)
        {
            if (isProvider)
            {
                return apprenticeships.Where(a =>
                (
                    //Has Any CourseDataLock Change Requested
                    a.DataLockStatus.Any(x =>
                    (
                        x.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                        || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                        || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                        || x.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)

                    ) && x.TriageStatus == TriageStatus.Restart && !x.IsResolved && !x.IsExpired)
                ));
            }
            else
            {
                return apprenticeships.Where(a =>
                (
                    //EmployerHasUnresolvedErrorsThatHaveKnownTriageStatus
                    a.DataLockStatus.Any(x =>
                        x.Status == Status.Fail &&
                        (x.TriageStatus != TriageStatus.Unknown && x.TriageStatus != TriageStatus.Change) &&
                        !x.IsResolved && !x.IsExpired)
                ));
            }
        }

        public static IQueryable<Apprenticeship> FilterApprenticeshipByAlertForChangesForReview(IQueryable<Apprenticeship> apprenticeships, Alerts alert, bool isProvider)
        {

            if (isProvider)
            {
                return apprenticeships.Where(a =>
                    a.ApprenticeshipUpdate != null &&
                    a.ApprenticeshipUpdate.Any(c => c.Originator == Originator.Employer && c.Status == ApprenticeshipUpdateStatus.Pending)
                );
            }

            return apprenticeships.Where(a =>
                a.ApprenticeshipUpdate != null &&
                a.ApprenticeshipUpdate.Any(c => c.Originator == Originator.Provider && c.Status == ApprenticeshipUpdateStatus.Pending)
            );

        }

    }

}
