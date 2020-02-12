using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using ApprenticeshipUpdateStatus = SFA.DAS.CommitmentsV2.Models.ApprenticeshipUpdateStatus;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class QueryableApprenticeshipsExtensions
    {
        public static IQueryable<Apprenticeship> Filter(this IQueryable<Apprenticeship> apprenticeships,
            ApprenticeshipSearchFilters filters)
        {
            if (filters == null)
            {
                return apprenticeships;
            }

            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                if(long.TryParse(filters.SearchTerm, out var result))
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
                                app.FirstName.StartsWith(firstName))
                            .Select(apprenticeship => apprenticeship.Id));
                        
                        found.AddRange(apprenticeships.Where(app =>
                                app.LastName.StartsWith(lastName))
                            .Select(apprenticeship => apprenticeship.Id));
                    }

                    apprenticeships = apprenticeships.Where(apprenticeship =>
                        found.Contains(apprenticeship.Id));
                }
            }

            if (!string.IsNullOrEmpty(filters.EmployerName))
            {
                apprenticeships = apprenticeships.Where(app => app.Cohort != null && filters.EmployerName.Equals(app.Cohort.LegalEntityName));
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

            return apprenticeships;
        }

        public static IQueryable<Apprenticeship> WithAlerts(this IQueryable<Apprenticeship> apprenticeships, bool hasAlerts)
        {
            if (hasAlerts)
            {
                return apprenticeships.Where(apprenticeship => apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != 3) || 
                                                                   apprenticeship.ApprenticeshipUpdate != null &&
                                                                   apprenticeship.ApprenticeshipUpdate.Any(
                                                                       c => c.Status == ApprenticeshipUpdateStatus.Pending 
                                                                            && (c.Originator == Originator.Employer 
                                                                                || c.Originator == Originator.Provider)));
            }

            return apprenticeships.Where(apprenticeship =>
                !apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != 3) &&
                (apprenticeship.ApprenticeshipUpdate == null ||
                apprenticeship.ApprenticeshipUpdate.All(c => c.Status != ApprenticeshipUpdateStatus.Pending)));
        }

        public static IQueryable<Apprenticeship> WithProviderOrEmployerId(
            this IQueryable<Apprenticeship> apprenticeships, ApprenticeshipSearchParameters searchParameters)
        {
            if (searchParameters.ProviderId.HasValue)
            {
                return apprenticeships.Where(app => app.Cohort.ProviderId == searchParameters.ProviderId);
            }

            return searchParameters.EmployerAccountId.HasValue ? 
                apprenticeships.Where(app => app.Cohort.EmployerAccountId == searchParameters.EmployerAccountId) : apprenticeships;
        }
    }
}