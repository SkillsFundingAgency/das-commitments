using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetPriceEpisodesResponseMapper : IMapper<GetPriceEpisodesQueryResult, GetPriceEpisodesResponse>
    {
        public Task<GetPriceEpisodesResponse> Map(GetPriceEpisodesQueryResult source)
        {
            return Task.FromResult(new GetPriceEpisodesResponse
            {
                PriceEpisodes = source.PriceEpisodes.Select(episode => new GetPriceEpisodesResponse.PriceEpisode
                    {
                        Id = episode.Id,
                        ApprenticeshipId = episode.ApprenticeshipId,
                        Cost = episode.Cost,
                        TrainingPrice = episode.TrainingPrice,
                        EndPointAssessmentPrice = episode.EndPointAssessmentPrice,
                        FromDate = episode.FromDate,
                        ToDate = episode.ToDate
                    }).ToList()
            });
        }
    }
}
