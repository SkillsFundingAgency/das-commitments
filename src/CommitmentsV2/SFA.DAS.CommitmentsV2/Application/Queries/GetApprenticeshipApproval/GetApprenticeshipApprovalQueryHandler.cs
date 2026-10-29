using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprenticeshipApprovalQuery, GetApprenticeshipApprovalQueryResult>
{
    public async Task<GetApprenticeshipApprovalQueryResult> Handle(GetApprenticeshipApprovalQuery request, CancellationToken cancellationToken)
    {
        var pendingItems = await dbContext.Value.CocApprovalItems
            .Where(item => item.ApprenticeshipId == request.ApprenticeshipId && item.Status == CocApprovalItemStatus.Pending)
            .OrderBy(item => item.Field)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return null;
        }

        var apprenticeship = await dbContext.Value.Apprenticeships
            .Include(x => x.Cohort).ThenInclude(x => x.Provider)
            .Include(a => a.Cohort).ThenInclude(c => c.AccountLegalEntity)
            .FirstOrDefaultAsync(x => x.Id == request.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            return null;
        }

        return new GetApprenticeshipApprovalQueryResult
        {
            ApprenticeshipId = apprenticeship.Id,
            ApprenticeshipStatus = apprenticeship.GetApprenticeshipStatus(null),
            Name = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
            ULN = apprenticeship.Uln,
            UKPRN = apprenticeship.Cohort.Provider.UkPrn,
            ProviderName = apprenticeship.Cohort.Provider.Name,
            AccountLegalEntityId = apprenticeship.Cohort.AccountLegalEntity.Id,
            AccountLegalEntityName = apprenticeship.Cohort.AccountLegalEntity.Name,
            AccountId = apprenticeship.Cohort.AccountLegalEntity.AccountId,
            StartDate = apprenticeship.OriginalStartDate ?? apprenticeship.StartDate,
            CourseCode = apprenticeship.CourseCode,
            CourseName = apprenticeship.CourseName,
            ApprovalRequestStatus = CocApprovalResultStatus.Pending,
            ApprovalRequestId = pendingItems[0].Id,
            Items = pendingItems.Select(item => new GetApprenticeshipApprovalQueryResult.ChangeItem
            {
                FieldName = item.Field,
                OldValue = item.Old,
                NewValue = item.New,
                EffectiveFromDate = null
            }).ToList()
        };
    }
}
