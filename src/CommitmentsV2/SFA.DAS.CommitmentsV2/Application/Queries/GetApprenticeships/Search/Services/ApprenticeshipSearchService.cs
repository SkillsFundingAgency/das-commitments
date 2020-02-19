using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;

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
            var totalApprenticeshipsWithoutAlerts = await GetApprenticeshipsWithFiltersQuery(searchParameters, false).CountAsync(searchParameters.CancellationToken);

            var totalApprenticeshipsWithAlerts = await GetApprenticeshipsWithFiltersQuery(searchParameters, true).CountAsync(searchParameters.CancellationToken);

            var totalApprenticeships = await GetApprenticeshipsQuery(searchParameters).CountAsync(searchParameters.CancellationToken);

            var skipCount = searchParameters.PageNumber > 0 ? (searchParameters.PageNumber - 1) * searchParameters.PageItemCount : 0;

            var apprentices = new List<Apprenticeship>();

            if (searchParameters.ReverseSort)
            {
                if (skipCount < totalApprenticeshipsWithoutAlerts)
                {
                    var apprenticeshipsWithoutAlerts =
                        await GetApprenticeshipsWithoutAlerts(searchParameters.CancellationToken, searchParameters, skipCount);

                    apprentices.AddRange(apprenticeshipsWithoutAlerts);
                }

                if (searchParameters.PageItemCount != 0 && apprentices.Count == searchParameters.PageItemCount)
                {
                    return new ApprenticeshipSearchResult
                    {
                        Apprenticeships = apprentices,
                        TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                        TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts,
                        TotalAvailableApprenticeships = totalApprenticeships
                    };
                }

                skipCount = skipCount - totalApprenticeshipsWithoutAlerts > 0
                    ? skipCount - totalApprenticeshipsWithoutAlerts
                   : 0;

                searchParameters.PageItemCount = searchParameters.PageItemCount - apprentices.Count > 0 ? searchParameters.PageItemCount - apprentices.Count : 0;

                var apprenticeshipsWithAlerts = await GetApprenticeshipsWithAlerts(searchParameters.CancellationToken, searchParameters, skipCount);

                apprentices.AddRange(apprenticeshipsWithAlerts);
            }
            else
            {
                if (skipCount < totalApprenticeshipsWithAlerts)
                {
                    var apprenticeshipsWithAlerts =
                        await GetApprenticeshipsWithAlerts(searchParameters.CancellationToken, searchParameters, skipCount);

                    apprentices.AddRange(apprenticeshipsWithAlerts);
                }

                if (searchParameters.PageItemCount != 0 && apprentices.Count >= searchParameters.PageItemCount)
                {
                    return new ApprenticeshipSearchResult
                    {
                        Apprenticeships = apprentices,
                        TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                        TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts,
                        TotalAvailableApprenticeships = totalApprenticeships
                    };
                }

                skipCount = skipCount - totalApprenticeshipsWithAlerts > 0
                    ? skipCount - totalApprenticeshipsWithAlerts
                    : 0;

                searchParameters.PageItemCount = searchParameters.PageItemCount - apprentices.Count > 0 ? searchParameters.PageItemCount - apprentices.Count : 0;

                var apprenticeshipsWithoutAlerts = await GetApprenticeshipsWithoutAlerts(searchParameters.CancellationToken, searchParameters,  skipCount);

                apprentices.AddRange(apprenticeshipsWithoutAlerts);
            }

            return new ApprenticeshipSearchResult
            {
                Apprenticeships = apprentices,
                TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts,
                TotalAvailableApprenticeships = totalApprenticeships
            };
        }

        private async Task<List<Apprenticeship>> GetApprenticeshipsWithoutAlerts(CancellationToken cancellationToken, ApprenticeshipSearchParameters searchParameters, int skipCount)
        {
            var query = GetApprenticeshipsWithFiltersQuery(searchParameters, false);

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

            if (searchParameters.PageItemCount > 0)
            {
                query = query.Take(searchParameters.PageItemCount);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private async Task<List<Apprenticeship>> GetApprenticeshipsWithAlerts(CancellationToken cancellationToken, ApprenticeshipSearchParameters searchParameters, int skipCount)
        {
            var query = GetApprenticeshipsWithFiltersQuery(searchParameters,  true);

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

            if (searchParameters.PageItemCount > 0)
            {
                query = query.Take(searchParameters.PageItemCount);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private IQueryable<Apprenticeship> GetApprenticeshipsQuery(ApprenticeshipSearchParameters searchParameters)
        { 
            return _dbContext
                .Apprenticeships
                .WithProviderOrEmployerId(searchParameters);
        }

        private IQueryable<Apprenticeship> GetApprenticeshipsWithFiltersQuery(ApprenticeshipSearchParameters searchParameters, bool withAlerts)
        { 
            return GetApprenticeshipsQuery(searchParameters)
                .WithAlerts(withAlerts)
                .Filter(searchParameters.Filters);
        }
    }
}
