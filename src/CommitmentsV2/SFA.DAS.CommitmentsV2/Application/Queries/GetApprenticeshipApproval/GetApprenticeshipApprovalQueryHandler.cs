using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;

public class GetApprenticeshipApprovalQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprenticeshipApprovalQuery, GetApprenticeshipApprovalQueryResult>
{
    public async Task<GetApprenticeshipApprovalQueryResult> Handle(GetApprenticeshipApprovalQuery request, CancellationToken cancellationToken)
    {
        var approvalRequest = await dbContext.Value.ApprovalRequests
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.ApprovalRequestId, cancellationToken);

        if (approvalRequest == null)
        {
            return null;
        }

        if (approvalRequest.ApprenticeshipId != request.ApprenticeshipId)
        {
            return null;
        }

        var apprenticeship = await dbContext.Value.Apprenticeships
            .Include(x => x.Cohort).ThenInclude(x => x.Provider)
            .Include(a => a.Cohort).ThenInclude(c => c.AccountLegalEntity)
            .FirstOrDefaultAsync(x => x.Id == request.ApprenticeshipId, cancellationToken);

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
            ApprovalRequestStatus = approvalRequest.Status,
            ApprovalRequestId = approvalRequest.Id,
            Items = approvalRequest.Items.Select(i => new GetApprenticeshipApprovalQueryResult.ChangeItem
            {
                FieldName = i.Field,
                OldValue = i.Old,
                NewValue = i.New,
                EffectiveFromDate = i.EffectiveFromDate
            }).ToList()
        };
    }
}