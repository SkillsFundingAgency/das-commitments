using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary;

public class GetApprenticeshipStatusSummaryQueryHandler(IApprenticeshipStatusSummaryService apprenticeshipStatusSummaryService)
    : IRequestHandler<GetApprenticeshipStatusSummaryQuery, GetApprenticeshipStatusSummaryQueryResults>
{
    public async Task<GetApprenticeshipStatusSummaryQueryResults> Handle(GetApprenticeshipStatusSummaryQuery request, CancellationToken cancellationToken)
    {           
        return await apprenticeshipStatusSummaryService.GetApprenticeshipStatusSummary(request.EmployerAccountId, cancellationToken);          
    }
}