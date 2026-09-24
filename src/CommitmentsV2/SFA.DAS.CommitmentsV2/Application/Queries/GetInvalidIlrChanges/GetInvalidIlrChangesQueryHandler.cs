using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetInvalidIlrChanges;

public class GetInvalidIlrChangesQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
    : IRequestHandler<GetInvalidIlrChangesQuery, GetInvalidIlrChangesResponse>
{
    public async Task<GetInvalidIlrChangesResponse> Handle(GetInvalidIlrChangesQuery query, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.Apprenticeships
            .AsNoTracking()
            .Include(a => a.Cohort)
            .Include(a => a.ApprovalRequests)
            .ThenInclude(request => request.Items)
            .SingleOrDefaultAsync(a => a.Id == query.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            return null;
        }

        if (apprenticeship.Cohort.ProviderId != query.ProviderId)
        {
            throw new UnauthorizedAccessException($"Provider {query.ProviderId} cannot access apprenticeship {query.ApprenticeshipId}");
        }

        var requestSets = (apprenticeship.ApprovalRequests ?? [])
            .Where(request => request.IsUnacknowledged(query.ItemStatus))
            .OrderByDescending(request => request.Created)
            .Select(request => new InvalidIlrChangeSet
            {
                ApprovalRequestId = request.Id,
                Decision = query.Decision,
                Fields = request.Items
                    .Where(item => item.Status == query.ItemStatus)
                    .Select(item => new InvalidIlrChangeField
                    {
                        Field = item.Field,
                        Old = item.Old,
                        New = item.New,
                        EffectiveFrom = item.EffectiveFromDate,
                        Reason = string.IsNullOrWhiteSpace(item.Reason) ? request.Reason : item.Reason
                    })
                    .ToList()
            })
            .ToList();

        return new GetInvalidIlrChangesResponse
        {
            FirstName = apprenticeship.FirstName,
            LastName = apprenticeship.LastName,
            RequestSets = requestSets
        };
    }
}
