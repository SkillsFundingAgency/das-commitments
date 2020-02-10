using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using ApprenticeshipUpdateStatus = SFA.DAS.CommitmentsV2.Models.ApprenticeshipUpdateStatus;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services
{
    public abstract class OrderedApprenticeshipSearchBaseService
    {
        protected static async Task<ApprenticeshipSearchResult> CreatePagedApprenticeshipSearchResult(CancellationToken cancellationToken, int pageNumber,
            int pageItemCount, IQueryable<Apprenticeship> apprenticeshipsQuery, int totalApprenticeshipsFound,
            int totalApprenticeshipsWithAlertsFound)
        {
            List<Apprenticeship> apprenticeships;

            if (pageItemCount < 1 || pageNumber < 1)
            {
                apprenticeships = await apprenticeshipsQuery.ToListAsync(cancellationToken);
            }
            else
            {
                apprenticeships = await apprenticeshipsQuery.Skip((pageNumber - 1) * pageItemCount)
                    .Take(pageItemCount)
                    .ToListAsync(cancellationToken);
            }

            return new ApprenticeshipSearchResult
            {
                Apprenticeships = apprenticeships,
                TotalApprenticeshipsFound = totalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlertsFound
            };
        }

        protected static Expression<Func<Apprenticeship, bool>> HasAlerts(long? providerId)
        {
            return apprenticeship => apprenticeship.Cohort.ProviderId == providerId
                                     && (apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != 3)
                                         || 
                                         apprenticeship.ApprenticeshipUpdate != null &&
                                         apprenticeship.ApprenticeshipUpdate.Any(
                                             c => c.Status == ApprenticeshipUpdateStatus.Pending 
                                                  && (c.Originator == Originator.Employer 
                                                      || c.Originator == Originator.Provider)
                                                  ));
        }

        protected Expression<Func<Apprenticeship, object>> GetOrderByField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Apprenticeship.FirstName):
                    return apprenticeship => apprenticeship.FirstName;
                case nameof(Apprenticeship.LastName):
                    return apprenticeship => apprenticeship.LastName;
                case nameof(Apprenticeship.CourseName):
                    return apprenticeship => apprenticeship.CourseName;
                case nameof(Apprenticeship.Cohort.LegalEntityName):
                    return apprenticeship => apprenticeship.Cohort.LegalEntityName;
                case nameof(Apprenticeship.StartDate):
                    return apprenticeship => apprenticeship.StartDate;
                case nameof(Apprenticeship.EndDate):
                    return apprenticeship => apprenticeship.EndDate;
                case nameof(Apprenticeship.ApprenticeshipStatus):
                    return apprenticeship => apprenticeship.PaymentStatus;
                case nameof(Apprenticeship.Uln):
                    return apprenticeship => apprenticeship.Uln;
                default:
                    return apprenticeship => apprenticeship.FirstName;
            }
        }

        protected Expression<Func<Apprenticeship, object>> GetSecondarySortByField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Apprenticeship.FirstName):
                    return apprenticeship => apprenticeship.LastName;
                default:
                    return GetOrderByField(fieldName);
            }
        }
    }
}
