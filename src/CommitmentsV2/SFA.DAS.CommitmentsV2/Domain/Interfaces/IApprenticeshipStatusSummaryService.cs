using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatistics;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IApprenticeshipStatusSummaryService
    {
        Task<GetApprenticeshipStatusSummaryQueryResults> GetApprenticeshipStatusSummary(long employerAccountId, CancellationToken cancellationToken);        
        Task<GetApprenticeshipStatisticsQueryResult> GetApprenticeshipStatisticsFor(int lastNumberOfDays);        
    }
}
