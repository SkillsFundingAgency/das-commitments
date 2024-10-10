using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IApprenticeshipStatusSummaryService
{
    Task<GetApprenticeshipStatusSummaryQueryResults> GetApprenticeshipStatusSummary(long employerAccountId, CancellationToken cancellationToken);        
}