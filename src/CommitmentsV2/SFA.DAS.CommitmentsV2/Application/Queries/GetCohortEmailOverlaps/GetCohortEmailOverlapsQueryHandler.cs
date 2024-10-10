using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps;

public class GetCohortEmailOverlapsQueryHandler(IOverlapCheckService overlapCheckService) : IRequestHandler<GetCohortEmailOverlapsQuery, GetCohortEmailOverlapsQueryResult>
{
    public async Task<GetCohortEmailOverlapsQueryResult> Handle(GetCohortEmailOverlapsQuery request, CancellationToken cancellationToken)
    {
        var results = await overlapCheckService.CheckForEmailOverlaps(request.CohortId, cancellationToken);
        
        return Map(results);
    }

    private static GetCohortEmailOverlapsQueryResult Map(IEnumerable<EmailOverlapCheckResult> results)
    {
        return new GetCohortEmailOverlapsQueryResult
        {
            Overlaps = results.Select(x => new ApprenticeshipEmailOverlap
            {
                Id = x.RowId,
                ErrorMessage = x.BuildErrorMessage()
            }).ToList()
        };
    }
}