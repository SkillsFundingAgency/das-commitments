using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services
{
    public class ReverseOrderedApprenticeshipSearchService : OrderedApprenticeshipSearchBaseService, IApprenticeshipSearchService<ReverseOrderedApprenticeshipSearchParameters>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public ReverseOrderedApprenticeshipSearchService(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApprenticeshipSearchResult> Find(ReverseOrderedApprenticeshipSearchParameters searchParameters)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .WithProviderOrEmployerId(searchParameters)
                .DownloadsFilter(searchParameters.PageNumber == 0);
                
            var totalAvailableApprenticeships = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);
                
            apprenticeshipsQuery = apprenticeshipsQuery.Filter(searchParameters.Filters);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.WithAlerts(true, searchParameters).CountAsync(searchParameters.CancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderByDescending(GetOrderByField(searchParameters.FieldName))
                .ThenByDescending(GetSecondarySortByField(searchParameters.FieldName))
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.PriceHistory)
                .Include(apprenticeship => apprenticeship.Cohort)
                    .ThenInclude(cohort => cohort.AccountLegalEntity)
                .Include(apprenticeship => apprenticeship.Cohort)
                    .ThenInclude(cohort => cohort.Provider)
                .Include(apprenticeship => apprenticeship.ConfirmationStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);

            return await CreatePagedApprenticeshipSearchResult(searchParameters.CancellationToken, searchParameters.PageNumber, searchParameters.PageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound, totalAvailableApprenticeships);
        }
    }
}
