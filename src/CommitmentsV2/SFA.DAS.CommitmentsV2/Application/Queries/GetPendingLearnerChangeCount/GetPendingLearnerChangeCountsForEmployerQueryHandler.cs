using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingLearnerChangeCount;

public class GetPendingLearnerChangeCountsForEmployerQueryHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<GetPendingLearnerChangeCountsForEmployerQuery, GetPendingLearnerChangeCountsForEmployerQueryResult>
{
    public async Task<GetPendingLearnerChangeCountsForEmployerQueryResult> Handle(GetPendingLearnerChangeCountsForEmployerQuery request, CancellationToken cancellationToken)
    {
        var apprenticeshipUpdates = db.Value.ApprenticeshipUpdates
            .Where(u => u.Apprenticeship.Cohort.EmployerAccountId == request.EmployerAccountId)
            .Where(u => u.Status == ApprenticeshipUpdateStatus.Pending
                        && u.Originator == Originator.Provider
            );

        var pendingCocRequests = db.Value.ApprovalRequests
            .Where(r => r.Apprenticeship.Cohort.EmployerAccountId == request.EmployerAccountId)
            .Where(r => r.Status == CocApprovalResultStatus.Pending);

        return new GetPendingLearnerChangeCountsForEmployerQueryResult
        {
            ManualPendingChangeCount = await apprenticeshipUpdates.CountAsync(cancellationToken),
            IlrPendingChangeCount = await pendingCocRequests.CountAsync(cancellationToken)
        };
    }
}

