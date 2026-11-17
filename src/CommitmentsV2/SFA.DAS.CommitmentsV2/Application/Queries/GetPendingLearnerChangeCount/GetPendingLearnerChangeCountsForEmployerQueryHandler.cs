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

        var ilrPendingChangeCount = await db.Value.CocApprovalItems
            .Where(item => item.Status == CocApprovalItemStatus.Pending)
            .Where(item => db.Value.Apprenticeships.Any(apprenticeship =>
                apprenticeship.Id == item.ApprenticeshipId
                && apprenticeship.Cohort.EmployerAccountId == request.EmployerAccountId))
            .Select(item => item.ApprenticeshipId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new GetPendingLearnerChangeCountsForEmployerQueryResult
        {
            ManualPendingChangeCount = await apprenticeshipUpdates.CountAsync(cancellationToken),
            IlrPendingChangeCount = ilrPendingChangeCount
        };
    }
}

