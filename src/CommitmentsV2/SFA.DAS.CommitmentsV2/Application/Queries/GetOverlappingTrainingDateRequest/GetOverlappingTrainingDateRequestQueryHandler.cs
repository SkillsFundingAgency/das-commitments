using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;

public class GetOverlappingTrainingDateRequestQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetOverlappingTrainingDateRequestQuery, GetOverlappingTrainingDateRequestQueryResult>
{
    public async Task<GetOverlappingTrainingDateRequestQueryResult> Handle(GetOverlappingTrainingDateRequestQuery request, CancellationToken cancellationToken)
    {
        var overlappingTrainingDateRequest = await dbContext.Value.OverlappingTrainingDateRequests
            .Where(x => x.PreviousApprenticeshipId == request.ApprenticeshipId)
            .ToListAsync(cancellationToken);

        if (overlappingTrainingDateRequest == null)
            return new GetOverlappingTrainingDateRequestQueryResult();

        return new GetOverlappingTrainingDateRequestQueryResult
        {
            OverlappingTrainingDateRequests = overlappingTrainingDateRequest.Select(x => new GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest
            {
                Id = x.Id,
                DraftApprenticeshipId = x.DraftApprenticeshipId,
                PreviousApprenticeshipId = x.PreviousApprenticeshipId,
                ResolutionType = x.ResolutionType,
                Status = x.Status,
                ActionedOn = x.ActionedOn,
                CreatedOn = x.CreatedOn,
            }).ToList()
        };
    }
}