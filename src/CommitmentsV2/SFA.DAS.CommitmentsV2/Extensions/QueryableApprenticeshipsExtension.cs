using System;
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

            if (filters?.StartDate != null)
            {
                apprenticeships = apprenticeships.Where(app => filters.StartDate.Equals(app.StartDate));
            }

            if (filters?.EndDate != null)
            {
                apprenticeships = apprenticeships.Where(app => filters.EndDate.Equals(app.EndDate));
            }

            return apprenticeships;
        }
    }
}
