using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using ApprenticeshipUpdateStatus = SFA.DAS.CommitmentsV2.Models.ApprenticeshipUpdateStatus;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services
{
    public class ApprenticeshipSearchService : IApprenticeshipSearchService<ApprenticeshipSearchParameters>
    {
        private readonly ICommitmentsReadOnlyDbContext _dbContext;

        public ApprenticeshipSearchService(ICommitmentsReadOnlyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApprenticeshipSearchResult> Find(ApprenticeshipSearchParameters searchParameters)
        {
            var totalApprenticeshipsWithoutAlerts = await GetApprenticeshipsQuery(searchParameters.ProviderId, searchParameters.Filters, false).CountAsync(searchParameters.CancellationToken);

            var totalApprenticeshipsWithAlerts = await GetApprenticeshipsQuery(searchParameters.ProviderId, searchParameters.Filters, true).CountAsync(searchParameters.CancellationToken);

            var skipCount = searchParameters.PageNumber > 0 ? (searchParameters.PageNumber - 1) * searchParameters.PageItemCount : 0;

            var apprentices = new List<Apprenticeship>();

            if (searchParameters.ReverseSort)
            {
                if (skipCount < totalApprenticeshipsWithoutAlerts)
                {
                    var apprenticeshipsWithoutAlerts =
                        await GetApprenticeshipsWithoutAlerts(searchParameters.CancellationToken, searchParameters.ProviderId, skipCount, searchParameters.PageItemCount, true, searchParameters.Filters);

                    apprentices.AddRange(apprenticeshipsWithoutAlerts);
                }

                if (searchParameters.PageItemCount != 0 && apprentices.Count == searchParameters.PageItemCount)
                {
                    return new ApprenticeshipSearchResult
                    {
                        Apprenticeships = apprentices,
                        TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                        TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts
                    };
                }

                skipCount = skipCount - totalApprenticeshipsWithoutAlerts > 0
                    ? skipCount - totalApprenticeshipsWithoutAlerts
                   : 0;

                searchParameters.PageItemCount = searchParameters.PageItemCount - apprentices.Count > 0 ? searchParameters.PageItemCount - apprentices.Count : 0;

                var apprenticeshipsWithAlerts = await GetApprenticeshipsWithAlerts(searchParameters.CancellationToken, searchParameters.ProviderId, skipCount, searchParameters.PageItemCount, true, searchParameters.Filters);

                apprentices.AddRange(apprenticeshipsWithAlerts);
            }
            else
            {
                if (skipCount < totalApprenticeshipsWithAlerts)
                {
                    var apprenticeshipsWithAlerts =
                        await GetApprenticeshipsWithAlerts(searchParameters.CancellationToken, searchParameters.ProviderId, skipCount, searchParameters.PageItemCount, false, searchParameters.Filters);

                    apprentices.AddRange(apprenticeshipsWithAlerts);
                }

                if (searchParameters.PageItemCount != 0 && apprentices.Count >= searchParameters.PageItemCount)
                {
                    return new ApprenticeshipSearchResult
                    {
                        Apprenticeships = apprentices,
                        TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                        TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts
                    };
                }

                skipCount = skipCount - totalApprenticeshipsWithAlerts > 0
                    ? skipCount - totalApprenticeshipsWithAlerts
                    : 0;

                searchParameters.PageItemCount = searchParameters.PageItemCount - apprentices.Count > 0 ? searchParameters.PageItemCount - apprentices.Count : 0;

                var apprenticeshipsWithoutAlerts = await GetApprenticeshipsWithoutAlerts(searchParameters.CancellationToken, searchParameters.ProviderId,  skipCount, searchParameters.PageItemCount, false, searchParameters.Filters);

                apprentices.AddRange(apprenticeshipsWithoutAlerts);
            }

            return new ApprenticeshipSearchResult
            {
                Apprenticeships = apprentices,
                TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts
            };
        }

        private async Task<List<Apprenticeship>> GetApprenticeshipsWithoutAlerts(CancellationToken cancellationToken, long? providerId, int skipCount, int pageItemCount, bool reverseOrder, ApprenticeshipSearchFilters filters)
        {
            var query = GetApprenticeshipsQuery(providerId, filters, false);

            query = query.OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort);

            if (skipCount > 0)
            {
                query = query.Skip(skipCount);
            }

            if (pageItemCount > 0)
            {
                query = query.Take(pageItemCount);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private async Task<List<Apprenticeship>> GetApprenticeshipsWithAlerts(CancellationToken cancellationToken, long? providerId, int skipCount, int pageItemCount, bool reverseOrder, ApprenticeshipSearchFilters filters)
        {
            var query = GetApprenticeshipsQuery(providerId, filters, true);

            query = query.OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate);

            if (skipCount > 0)
            {
                query = query.Skip(skipCount);
            }

            if (pageItemCount > 0)
            {
                query = query.Take(pageItemCount);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private IQueryable<Apprenticeship> GetApprenticeshipsQuery(long? providerId, ApprenticeshipSearchFilters filters, bool withAlerts)
        {
            return _dbContext
                .Apprenticeships
                .Where(withAlerts ? HasAlerts(providerId) : HasNoAlerts(providerId))
                .Filter(filters);
        }

        private static Expression<Func<Apprenticeship, bool>> HasAlerts(long? providerId)
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

        private static Expression<Func<Apprenticeship, bool>> HasNoAlerts(long? providerId)
        {
            return apprenticeship => apprenticeship.Cohort.ProviderId == providerId
                                     && !apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != 3)
                                     && apprenticeship.ApprenticeshipUpdate.All(c => c.Status != ApprenticeshipUpdateStatus.Pending);
        }
    }
}
