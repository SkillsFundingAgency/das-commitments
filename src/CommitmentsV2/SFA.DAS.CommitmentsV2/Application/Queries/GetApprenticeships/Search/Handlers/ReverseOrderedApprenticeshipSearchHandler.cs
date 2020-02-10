using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Handlers
{
    public class ReverseOrderedApprenticeshipSearchHandler : OrderedApprenticeshipSearchBaseHandler, IApprenticeshipSearchHandler<ReverseOrderedApprenticeshipSearchParameters>
    {
        private readonly ICommitmentsReadOnlyDbContext _dbContext;

        public ReverseOrderedApprenticeshipSearchHandler(ICommitmentsReadOnlyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApprenticeshipSearchResult> Find(ReverseOrderedApprenticeshipSearchParameters searchParameters)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == searchParameters.ProviderId)
                .Filter(searchParameters.Filters);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(HasAlerts(searchParameters.ProviderId), searchParameters.CancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderByDescending(GetOrderByField(searchParameters.FieldName))
                .ThenByDescending(GetSecondarySortByField(searchParameters.FieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(searchParameters.CancellationToken);

            return await CreatePagedApprenticeshipSearchResult(searchParameters.CancellationToken, searchParameters.PageNumber, searchParameters.PageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound);
        }
    }
}
