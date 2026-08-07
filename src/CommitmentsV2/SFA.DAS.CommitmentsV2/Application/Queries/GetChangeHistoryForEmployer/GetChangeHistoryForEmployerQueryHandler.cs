using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistoryForEmployer;

public class GetChangeHistoryForEmployerQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetChangeHistoryForEmployerQuery, GetChangeHistoryForEmployerQueryResult>
{
    public async Task<GetChangeHistoryForEmployerQueryResult> Handle(GetChangeHistoryForEmployerQuery request, CancellationToken cancellationToken)
    {
        var changeHistories = await dbContext.Value.LearningChangeHistory.AsNoTracking()
           .Where(x => x.AccountId == request.AccountId)
           .Select(x => new ChangeHistory
           {
               ChangeType = x.ChangeType,
               Description = x.Description,
               ApprenticeshipId = x.ApprenticeshipId,
               LearnerName = x.LearnerName,
               AppliedDate = x.AppliedDate,
               Created = x.Created,
               Id = x.Id,
               ProviderName = x.ProviderName
           }).
           OrderByDescending(x => x.Created).
           ToListAsync(cancellationToken);

        return new GetChangeHistoryForEmployerQueryResult
        {
            ChangeHistory = changeHistories
        };
    }
}