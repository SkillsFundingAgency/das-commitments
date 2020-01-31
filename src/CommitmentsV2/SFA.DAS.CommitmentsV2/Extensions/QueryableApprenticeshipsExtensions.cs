using System.Linq;
using SFA.DAS.CommitmentsV2.Models;

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
                var paymentStatuses = filters.Status.Value.MapToPaymentStatuses();

                apprenticeships = apprenticeships.Where(app => paymentStatuses.Any(s => s.Equals(app.PaymentStatus)));
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
