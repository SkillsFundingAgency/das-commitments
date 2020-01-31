using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class QueryableApprenticeshipsExtension
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

                    found.AddRange(apprenticeships.Where(app => 
                        app.FirstName.StartsWith(filters.SearchTerm))
                        .Select(apprenticeship => apprenticeship.Id));

                    found.AddRange(apprenticeships.Where(app => 
                        app.LastName.StartsWith(filters.SearchTerm))
                        .Select(apprenticeship => apprenticeship.Id));

                    apprenticeships = apprenticeships.Where(apprenticeship =>
                        found.Contains(apprenticeship.Id));
                }
            }

            if (!string.IsNullOrEmpty(filters?.EmployerName))
            {
                apprenticeships = apprenticeships.Where(app => app.Cohort != null && filters.EmployerName.Equals(app.Cohort.LegalEntityName));
            }

            if (!string.IsNullOrEmpty(filters?.CourseName))
            {
                apprenticeships = apprenticeships.Where(app => filters.CourseName.Equals(app.CourseName));
            }

            if (!string.IsNullOrEmpty(filters?.Status) && Enum.TryParse(filters.Status, out PaymentStatus paymentStatus))
            {
                apprenticeships = apprenticeships.Where(app => paymentStatus.Equals(app.PaymentStatus));
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
    }
}
