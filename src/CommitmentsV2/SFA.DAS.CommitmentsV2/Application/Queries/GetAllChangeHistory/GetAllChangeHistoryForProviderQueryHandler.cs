using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllChangeHistory;

public class GetAllChangeHistoryForProviderQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetAllChangeHistoryForProviderQuery, GetAllChangeHistoryForProviderQueryResult>
{
    public async Task<GetAllChangeHistoryForProviderQueryResult> Handle(GetAllChangeHistoryForProviderQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Value.LearningChangeHistory.AsNoTracking()
           .Where(x => x.UKPRN == request.ProviderId);

        var totalCount = await query.CountAsync(cancellationToken);

        var availableFrom = await query.MinAsync(x => (DateTime?)x.Created, cancellationToken);

        var changeHistories = await query
           .OrderByDescending(x => x.Created)
           .Skip((request.PageNumber - 1) * request.PageSize)
           .Take(request.PageSize)
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
           })
           .ToListAsync(cancellationToken);

        return new GetAllChangeHistoryForProviderQueryResult
        {
            ChangeHistory = changeHistories,
            TotalCount = totalCount,
            AvailableFrom = availableFrom
        };
    }
}