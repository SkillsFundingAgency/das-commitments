using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;

public class GetChangeHistoryQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetChangeHistoryQuery, GetChangeHistoryQueryResult>
{
    public async Task<GetChangeHistoryQueryResult> Handle(GetChangeHistoryQuery query, CancellationToken cancellationToken)
    {
        var changeHistories = await dbContext.Value.LearningChangeHistory
            .Where(x => x.ApprenticeshipId == query.ApprenticeshipId)
            .Select(x => new ChangeHistory
            {
                ChangeType = x.ChangeType,
                Description = x.Description,
                ApprenticeshipId = x.ApprenticeshipId,
                LearnerName = x.LearnerName,
                AppliedDate = x.AppliedDate,
                Id = x.Id,
            }).ToListAsync(cancellationToken);

        return new GetChangeHistoryQueryResult
        {
            ChangeHistory = changeHistories
        };
    }
}