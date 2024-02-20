using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services
{
    public class OrderedApprenticeshipSearchService : OrderedApprenticeshipSearchBaseService, IApprenticeshipSearchService<OrderedApprenticeshipSearchParameters>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public OrderedApprenticeshipSearchService(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApprenticeshipSearchResult> Find(OrderedApprenticeshipSearchParameters searchParameters)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .WithProviderOrEmployerId(searchParameters)
                .DownloadsFilter(searchParameters.PageNumber == 0);

            var totalAvailableApprenticeships = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery.Filter(searchParameters.Filters);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.WithAlerts(true, searchParameters).CountAsync(searchParameters.CancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderBy(GetOrderByField(searchParameters.FieldName))
                .ThenBy(GetSecondarySortByField(searchParameters.FieldName))
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.PriceHistory)
                .Include(apprenticeship => apprenticeship.Cohort)
                    .ThenInclude(cohort => cohort.AccountLegalEntity)
                .Include(apprenticeship => apprenticeship.Cohort)
                    .ThenInclude(cohort => cohort.Provider)
                .Include(apprenticeship => apprenticeship.ApprenticeshipConfirmationStatus);
            

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);

            return await CreatePagedApprenticeshipSearchResult(searchParameters.CancellationToken, searchParameters.PageNumber, searchParameters.PageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound, totalAvailableApprenticeships);
        }
    }
}
