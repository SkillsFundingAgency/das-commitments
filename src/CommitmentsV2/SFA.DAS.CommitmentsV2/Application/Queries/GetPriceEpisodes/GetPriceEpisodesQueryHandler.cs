using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;

public class GetPriceEpisodesQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetPriceEpisodesQuery, GetPriceEpisodesQueryResult>
{
    public async Task<GetPriceEpisodesQueryResult> Handle(GetPriceEpisodesQuery request, CancellationToken cancellationToken)
    {
        return new GetPriceEpisodesQueryResult
        {
            PriceEpisodes = await dbContext.Value.PriceHistory
                .Where(x => x.ApprenticeshipId == request.ApprenticeshipId)
                .Select(a => new GetPriceEpisodesQueryResult.PriceEpisode
                {
                    Id = a.Id,
                    ApprenticeshipId = a.ApprenticeshipId,
                    FromDate = a.FromDate,
                    ToDate = a.ToDate,
                    Cost = a.Cost,
                    TrainingPrice = a.TrainingPrice,
                    EndPointAssessmentPrice = a.AssessmentPrice
                }).ToListAsync(cancellationToken)
        };
    }
}