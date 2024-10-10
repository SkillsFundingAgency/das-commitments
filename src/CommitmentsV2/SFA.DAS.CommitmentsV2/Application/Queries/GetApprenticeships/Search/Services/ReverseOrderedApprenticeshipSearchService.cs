using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;

public class ReverseOrderedApprenticeshipSearchService(IProviderCommitmentsDbContext dbContext) : OrderedApprenticeshipSearchBaseService, IApprenticeshipSearchService<ReverseOrderedApprenticeshipSearchParameters>
{
    public async Task<ApprenticeshipSearchResult> Find(ReverseOrderedApprenticeshipSearchParameters searchParameters)
    {
        var apprenticeshipsQuery = dbContext
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
            .Include(apprenticeship => apprenticeship.ApprenticeshipConfirmationStatus);

        var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);

        return await CreatePagedApprenticeshipSearchResult(searchParameters.PageNumber, searchParameters.PageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound, totalAvailableApprenticeships, searchParameters.CancellationToken);
    }
}