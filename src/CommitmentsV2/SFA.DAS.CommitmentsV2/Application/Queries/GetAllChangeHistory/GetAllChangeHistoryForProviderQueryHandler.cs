using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllChangeHistory;

public class GetAllChangeHistoryForProviderQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetAllChangeHistoryForProviderQuery, GetAllChangeHistoryForProviderQueryResult>
{
    public async Task<GetAllChangeHistoryForProviderQueryResult> Handle(GetAllChangeHistoryForProviderQuery request, CancellationToken cancellationToken)
    {
        var changeHistories = await dbContext.Value.LearningChangeHistory
           .Where(x => x.UKPRN == request.ProviderId)
           .Select(x => new ChangeHistory
           {
               ChangeType = x.ChangeType,
               Description = x.Description,
               ApprenticeshipId = x.ApprenticeshipId,
               LearnerName = x.LearnerName,
               AppliedDate = x.AppliedDate,
               Created = x.Created,
               Id = x.Id,
               EmployerName = x.EmployerName
           }).ToListAsync(cancellationToken);

        return new GetAllChangeHistoryForProviderQueryResult
        {
            ChangeHistory = changeHistories
        };
    }
}