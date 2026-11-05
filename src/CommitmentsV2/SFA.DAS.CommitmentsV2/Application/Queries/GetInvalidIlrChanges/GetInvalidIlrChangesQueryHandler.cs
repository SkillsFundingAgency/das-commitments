using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
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
            .SingleOrDefaultAsync(a => a.Id == query.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            return null;
        }

        if (apprenticeship.Cohort.ProviderId != query.ProviderId)
        {
            throw new UnauthorizedAccessException($"Provider {query.ProviderId} cannot access apprenticeship {query.ApprenticeshipId}");
        }

        var fieldRows = await dbContext.Value.CocApprovalItems
            .AsNoTracking()
            .Where(item => item.ApprenticeshipId == query.ApprenticeshipId
                && item.Status == query.ItemStatus
                && item.ProviderAcknowledgedAt == null)
            .OrderByDescending(item => item.Created)
            .ToListAsync(cancellationToken);

        var requestSets = fieldRows
            .Select(item => new InvalidIlrChangeSet
            {
                ApprovalRequestId = item.Id,
                Decision = query.Decision,
                Fields =
                [
                    new InvalidIlrChangeField
                    {
                        Field = item.Field,
                        Old = item.Old,
                        New = item.New,
                        Reason = item.Reason
                    }
                ]
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
