using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services
{
    public class OrderedApprenticeshipSearchService : OrderedApprenticeshipSearchBaseService, IApprenticeshipSearchService<OrderedApprenticeshipSearchParameters>
    {
        private readonly ICommitmentsReadOnlyDbContext _dbContext;

        public OrderedApprenticeshipSearchService(ICommitmentsReadOnlyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApprenticeshipSearchResult> Find(OrderedApprenticeshipSearchParameters searchParameters)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .WithProviderOrEmployerId(searchParameters)
                .Filter(searchParameters.Filters);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.WithAlerts(true).CountAsync(searchParameters.CancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderBy(GetOrderByField(searchParameters.FieldName))
                .ThenBy(GetSecondarySortByField(searchParameters.FieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);

            return await CreatePagedApprenticeshipSearchResult(searchParameters.CancellationToken, searchParameters.PageNumber, searchParameters.PageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound);
        }
    }
}
