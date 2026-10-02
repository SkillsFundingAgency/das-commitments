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

        var alertCounts = await apprenticeshipsQuery.CountAlertsAsync(searchParameters, searchParameters.CancellationToken);

        apprenticeshipsQuery = apprenticeshipsQuery
            .OrderByDescending(GetOrderByField(searchParameters.FieldName))
            .ThenByDescending(GetSecondarySortByField(searchParameters.FieldName))
            .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
            .Include(apprenticeship => apprenticeship.DataLockStatus)
            .Include(apprenticeship => apprenticeship.PriceHistory)
            .Include(apprenticeship => apprenticeship.EmployerVerificationRequest)
            .Include(apprenticeship => apprenticeship.Cohort)
            .ThenInclude(cohort => cohort.AccountLegalEntity)
            .Include(apprenticeship => apprenticeship.Cohort)
            .ThenInclude(cohort => cohort.Provider)
            .Include(apprenticeship => apprenticeship.ApprenticeshipConfirmationStatus)
            .Include(apprenticeship => apprenticeship.ApprovalRequests)
            .ThenInclude(request => request.Items);

        return await CreatePagedApprenticeshipSearchResult(searchParameters.PageNumber, searchParameters.PageItemCount, apprenticeshipsQuery, alertCounts.Total, alertCounts.WithAlerts, totalAvailableApprenticeships, searchParameters.CancellationToken);
    }
}